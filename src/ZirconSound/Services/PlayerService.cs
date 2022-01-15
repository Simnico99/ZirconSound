using Asyncronizer.Tasks;
using Lavalink4NET.Events;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using ZirconSound.Helpers;

namespace ZirconSound.Services;

public class PlayerService : IPlayerService
{
    private readonly ConcurrentDictionary<ulong?, CancellationTokenSource> _aloneDisconnectTokens;
    private readonly ConcurrentDictionary<ulong?, CancellationTokenSource> _disconnectTokens;
    private readonly IAudioService _audioService;
    private readonly ILogger _logger;
    private readonly LockHelper _lockHelper;

    public PlayerService(IAudioService audioService, ILogger<PlayerService> logger, LockHelper lockHelper)
    {
        _disconnectTokens = new ConcurrentDictionary<ulong?, CancellationTokenSource>();
        _aloneDisconnectTokens = new ConcurrentDictionary<ulong?, CancellationTokenSource>();
        _logger = logger;
        _audioService = audioService;
        _lockHelper = lockHelper;

        audioService.TrackEnd += AudioService_TrackEnd;
        audioService.TrackStuck += AudioService_TrackStuck;
        audioService.TrackStarted += AudioService_TrackStarted;
        audioService.TrackException += AudioService_TrackException;
    }

    private async Task AudioService_TrackException(object sender, TrackExceptionEventArgs eventArgs)
    {
        if (eventArgs.Player is ZirconPlayer player)
        {
            _logger.LogWarning("Track {TackTitle} threw an exception. Please check Lavalink console/logs.", player.CurrentTrack?.Title);

            var embed = EmbedHandler.Create(player.Context);
            embed.AddField("Error", $"Cannot play:\n{player.CurrentTrack?.Title} because of an error.");
            await player.Context.ReplyToCommandAsync(embed: embed.BuildSync(ZirconEmbedType.Error));

            if (player.Queue.Count == 0)
            {
                await player.StopAsync();
                await InitiateDisconnectAsync(player, TimeSpan.FromSeconds(40));
            }
            else
            {
                await player.SkipAsync();
            }
        }
    }

    private async Task AudioService_TrackStuck(object sender, TrackStuckEventArgs eventArgs)
    {

        if (eventArgs.Player is ZirconPlayer player)
        {

            _logger.LogWarning("Track {TrackTitle} got stuck. Please check Lavalink console/logs.", player.CurrentTrack?.Title);
            if (player.IsLooping)
            {
                if (player.CurrentTrack != null)
                {
                    var currentSong = eventArgs.TrackIdentifier;
                    var track = await _audioService.GetTrackAsync(currentSong, SearchMode.YouTube, true);
                    if (track != null)
                    {
                        await player.PlayAsync(track);
                    }
                    else
                    {
                        await player.StopAsync();
                        await InitiateDisconnectAsync(player, TimeSpan.FromSeconds(40));
                    }
                }
                else
                {
                    await player.StopAsync();
                    await InitiateDisconnectAsync(player, TimeSpan.FromSeconds(40));
                }
            }

            if (player.Queue.Count == 0 && !player.IsLooping)
            {
                await player.StopAsync();
                await InitiateDisconnectAsync(player, TimeSpan.FromSeconds(40));
            }
            else
            {
                await player.SkipAsync();
            }
        }
    }

    private async Task AudioService_TrackStarted(object sender, TrackStartedEventArgs eventArgs) => await CancelDisconnectAsync(eventArgs.Player);

    private async Task AudioService_TrackEnd(object sender, TrackEndEventArgs eventArgs)
    {
        _logger.LogDebug("Stop reason: {Reason}", eventArgs.Reason);
        if (eventArgs.Reason != TrackEndReason.LoadFailed)
        {
            if (eventArgs.Player is ZirconPlayer player)
            {
                if (player.IsLooping)
                {
                    await player.ReplayAsync();
                    return;
                }

                if (player.Queue.IsEmpty && eventArgs.Reason != TrackEndReason.Replaced)
                {
                    player.Queue.Clear();
                    await player.StopAsync();
                    await InitiateDisconnectAsync(eventArgs.Player, TimeSpan.FromSeconds(40));
                }
            }
        }
        else
        {
            if (eventArgs.Player is ZirconPlayer player)
            {
                if (player.ErrorRetry >= 5 && player.LastErrorTrack == player.CurrentTrack)
                {
                    var embed = EmbedHandler.Create(player.Context);
                    embed.AddField("Error", $"Cannot play:\n{player.CurrentTrack?.Title} because of an error.");
                    await player.Context.ReplyToCommandAsync(embed: embed.BuildSync(ZirconEmbedType.Error));

                    await InitiateDisconnectAsync(eventArgs.Player, TimeSpan.FromSeconds(40));
                    await player.SkipAsync();
                }
                else
                {
                    if (player.LastErrorTrack != player.CurrentTrack)
                    {
                        player.LastErrorTrack = player.CurrentTrack;
                        player.ErrorRetry = 0;
                    }

                    player.ErrorRetry++;

                    await player.PlayAsync(player.CurrentTrack);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
        }
    }

    public async Task CancelDisconnectAsync(LavalinkPlayer player)
    {
        if (player is ZirconPlayer zirconplayer)
        {
            _logger.LogDebug("Canceling Disconnect");
            if (!_disconnectTokens.TryGetValue(zirconplayer.VoiceChannelId ?? 0, out var value))
            {
                value = new CancellationTokenSource();
            }
            else if (value.IsCancellationRequested)
            {
                return;
            }

            await Task.Run(() => value.Cancel(true));
            _logger.LogDebug("Canceled Disconnect");
        }
    }



    public async Task CancelAloneDisconnectAsync(LavalinkPlayer player)
    {
        _logger.LogDebug("Canceling Alone Disconnect");
        if (player is ZirconPlayer zirconplayer)
        {
            await zirconplayer.Locker.LockAsync(async () =>
            {
                if (!_aloneDisconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value))
                {
                    value = new CancellationTokenSource();
                }
                else if (value.IsCancellationRequested)
                {
                    return;
                }

                await Task.Run(() => value.Cancel(true));
                _logger.LogDebug("Canceled Alone Disconnect");
            });
        }
    }

    public async Task InitiateDisconnectAsync(LavalinkPlayer player, TimeSpan timeSpan)
    {
        _logger.LogDebug("Disconnecting");
        if (!_disconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value))
        {
            value = new CancellationTokenSource();
            _disconnectTokens.TryAdd(player.VoiceChannelId, value);
        }
        else if (value.IsCancellationRequested)
        {
            _disconnectTokens.TryUpdate(player.VoiceChannelId, new CancellationTokenSource(), value);
            value = _disconnectTokens[player.VoiceChannelId ?? 0];
        }

        await TaskAsync.Run(async () =>
        {
            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await player.DisconnectAsync();
            _logger.LogDebug("Disconnected");
        });
    }

    public async Task BotIsAloneAsync(LavalinkPlayer player, TimeSpan timeSpan)
    {
        _logger.LogDebug("Alone Disconnecting");
        if (!_aloneDisconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value))
        {
            value = new CancellationTokenSource();
            _aloneDisconnectTokens.TryAdd(player.VoiceChannelId, value);
        }
        else if (value.IsCancellationRequested)
        {
            _aloneDisconnectTokens.TryUpdate(player.VoiceChannelId, new CancellationTokenSource(), value);
            value = _aloneDisconnectTokens[player.VoiceChannelId ?? 0];
        }

        await TaskAsync.Run(async () =>
        {
            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await player.DisconnectAsync();
            _logger.LogDebug("Alone Disconnected");
        });
    }
}
