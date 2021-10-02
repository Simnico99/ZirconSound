using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Microsoft.Extensions.Logging;
using ZirconSound.Embeds;

namespace ZirconSound.Services
{
    public class PlayerService
    {
        private readonly ConcurrentDictionary<ulong?, CancellationTokenSource> _aloneDisconnectTokens;
        private readonly ConcurrentDictionary<ulong?, CancellationTokenSource> _disconnectTokens;
        private readonly EmbedHandler _embedHandler;
        private readonly IAudioService _audioService;
        private readonly ILogger _logger;

        public PlayerService(EmbedHandler embedHandler, IAudioService audioService, ILogger<PlayerService> logger)
        {
            _disconnectTokens = new ConcurrentDictionary<ulong?, CancellationTokenSource>();
            _aloneDisconnectTokens = new ConcurrentDictionary<ulong?, CancellationTokenSource>();
            _logger = logger;
            _embedHandler = embedHandler;
            _audioService = audioService;

            audioService.TrackEnd += AudioService_TrackEnd;
            audioService.TrackStuck += AudioService_TrackStuck;
            audioService.TrackStarted += AudioService_TrackStarted;
            audioService.TrackException += AudioService_TrackException;
        }

        private async Task AudioService_TrackException(object sender, TrackExceptionEventArgs eventArgs)
        {
            var player = (QueuedLavalinkPlayer)eventArgs.Player;

            _logger.LogWarning($"Track {eventArgs.Player.CurrentTrack?.Title} threw an exception. Please check Lavalink console/logs.");
            var embed = _embedHandler.Create();
            embed.AddField("Error:", $"{player.CurrentTrack?.Title} has been skipped after throwing an exception.");
            embed.AddField("Error Message:", eventArgs.Exception.Message);
            embed.AddField("Possible causes:", "The video could be age restricted, etc...");


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

        private async Task AudioService_TrackStuck(object sender, TrackStuckEventArgs eventArgs)
        {
            _logger.LogWarning($"Track {eventArgs.Player.CurrentTrack?.Title} got stuck. Please check Lavalink console/logs.");
            var player = (QueuedLavalinkPlayer)eventArgs.Player;

            if (player.IsLooping)
            {
                if (player.CurrentTrack != null)
                {
                    var currentSong = player.CurrentTrack.Source;
                    var track = await _audioService.GetTrackAsync(currentSong ?? string.Empty, SearchMode.YouTube, true);
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
                return;
            }

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

        private async Task AudioService_TrackStarted(object sender, TrackStartedEventArgs eventArgs) => await CancelDisconnectAsync(eventArgs.Player);

        private async Task AudioService_TrackEnd(object sender, TrackEndEventArgs eventArgs)
        {
            var player = (QueuedLavalinkPlayer)eventArgs.Player;

            if (player.IsLooping)
            {
                await player.ReplayAsync();
                return;
            }

            if (player.Queue.IsEmpty)
            {
                await InitiateDisconnectAsync(eventArgs.Player, TimeSpan.FromSeconds(40));
            }
        }

        public async Task CancelDisconnectAsync(LavalinkPlayer player)
        {
            _logger.LogDebug("Canceling Disconnect");
            if (!_disconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value))
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

        public async Task CancelAloneDisconnectAsync(LavalinkPlayer player)
        {
            _logger.LogDebug("Canceling Alone Disconnect");
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
        }

        private async Task InitiateDisconnectAsync(LavalinkPlayer player, TimeSpan timeSpan)
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

            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await player.DisconnectAsync();
            _logger.LogDebug("Disconnected");
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

            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await player.DisconnectAsync();
            _logger.LogDebug("Alone Disconnected");
        }
    }
}