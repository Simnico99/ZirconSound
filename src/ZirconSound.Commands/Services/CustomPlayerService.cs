using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using System.Numerics;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;

namespace ZirconSound.Application.Services;
public sealed class CustomPlayerService : ICustomPlayerService
{
    private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _aloneDisconnectTokens;
    private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;

    private readonly ILogger _logger;
    private readonly IAudioService _audioService;

    public CustomPlayerService(IAudioService audioService, ILogger<CustomPlayerService> logger)
    {
        _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        _aloneDisconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        _audioService = audioService;
        _logger = logger;

        _audioService.TrackEnd += AudioService_TrackEnd;
    }


    private async Task OnError(GenericQueuedLavalinkPlayer player, TrackEndReason trackEndReason)
    {
        _logger.LogWarning("Track {TackTitle} threw an exception.", player.CurrentTrack?.Title);

        if (player.PlayerGotError || player.CurrentTrack is null)
        {
            player.PlayerGotError = false;
            string? errorMessage;

            if (player.CurrentTrack is not null && trackEndReason is not TrackEndReason.Replaced)
            {
                errorMessage = $"An error occured with a song ({player.CurrentTrack.Title}):\nThe song will be skipped and removed from queue.";
                player.Queue.Remove(player.CurrentTrack);
            }
            else
            {
                errorMessage = $"An error occured with a song (Unable to get the song title):\nThe song will be skipped.";
            }

            if (player.Context is not null)
            {
                var embed = EmbedHelpers.CreateGenericEmbedBuilder(player.Context);
                embed.AddField("Error", errorMessage);
                await player.Context.Interaction.FollowupAsync(embed: embed.Build(GenericEmbedType.Error));
            }

            return;
        }

        player.PlayerGotError = true;
        await player.PlayAsync(player.CurrentTrack);
    }

    private async Task AudioService_TrackEnd(object sender, TrackEndEventArgs eventArgs)
    {
        _logger.LogDebug("Stop reason: {Reason}", eventArgs.Reason);

        if (eventArgs.Player is not GenericQueuedLavalinkPlayer player)
        {
            return;
        }

        if (eventArgs.Reason is not TrackEndReason.LoadFailed and not TrackEndReason.Replaced)
        {

            if (player.CurrentLoopingTrack is not null)
            {
                await player.PlayAsync(player.CurrentLoopingTrack);
                return;
            }

            await DisconnectAndStopIfNoTracksRemainingAsync(player);
        }

        if (eventArgs.Reason is TrackEndReason.LoadFailed or TrackEndReason.Replaced && !player.SkippedOnPurpose)
        {
            await DisconnectAndStopIfNoTracksRemainingAsync(player, true, eventArgs.Reason);
        }
    }

    private async Task DisconnectAndStopIfNoTracksRemainingAsync(GenericQueuedLavalinkPlayer player, bool errorCheck = false, TrackEndReason trackEndReason = TrackEndReason.Finished)
    {
        if (errorCheck)
        {
            await OnError(player, trackEndReason);
        }

        if (player.PlayerGotError)
        {
            return;
        }

        if (player.CurrentLoopingPlaylist is not null && player.CurrentLoopingPlaylist.Count > 0)
        {
            if (player.CurrentTrack == player.CurrentLoopingPlaylist.Last())
            {
                player.Queue.Clear();
                await player.PlayAsync(player.CurrentLoopingPlaylist.First());
                var playlist = player.CurrentLoopingPlaylist.Skip(1);

                foreach (var track in playlist)
                {
                    player.Queue.Add(track);
                }

                return;
            }
        }


        if (player.Queue is null || player.Queue.Count <= 0 || player.Queue.Last() == player.CurrentTrack || player.State != PlayerState.Playing)
        {
            await player.StopAsync();
            StartIdleDisconnectTimer(player, TimeSpan.FromSeconds(30));
            return;
        }

        await player.SkipAsync();
    }

    public void StartDisconnectBotIsAloneTimer(LavalinkPlayer? player, TimeSpan timeSpan)
    {
        _logger.LogDebug("Disconnecting alone timer started waiting: {time}", timeSpan);

        if (player is null)
        {
            return;
        }

        var cancellationSourceGetResult = _aloneDisconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value);
        value?.Cancel();

        if (!cancellationSourceGetResult || value is null)
        {
            value = new CancellationTokenSource();
            _aloneDisconnectTokens.TryAdd((ulong)player.VoiceChannelId!, value);
        }

        _ = Task.Run(async () =>
        {
            await Task.Delay(timeSpan, value.Token);

            if (!value.Token.IsCancellationRequested)
            {
                await player.DisconnectAsync();
                _logger.LogDebug("Alone Disconnected");
            }
        });
    }

    public void StartIdleDisconnectTimer(LavalinkPlayer? player, TimeSpan timeSpan)
    {
        _logger.LogDebug("Disconnecting idle timer started waiting: {time}", timeSpan);

        if (player is null)
        {
            return;
        }

        var cancellationSourceGetResult = _disconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value);
        value?.Cancel();

        if (!cancellationSourceGetResult || value is null)
        {
            value = new CancellationTokenSource();
            _disconnectTokens.TryAdd((ulong)player.VoiceChannelId!, value);
        }

        _ = Task.Run(async () =>
        {
            await Task.Delay(timeSpan, value.Token);

            if (!value.Token.IsCancellationRequested)
            {
                await player.DisconnectAsync();
                _logger.LogDebug("Alone Disconnected");
            }
        });
    }

    public void CancelAloneDisconnect(LavalinkPlayer? player)
    {
        _logger.LogDebug("Canceling alone disconnect");
        if (player is GenericQueuedLavalinkPlayer zirconplayer)
        {
            var cancellationSourceGetResult = _aloneDisconnectTokens.TryGetValue(zirconplayer.VoiceChannelId ?? 0, out var value);

            if (value is null || value.IsCancellationRequested || !cancellationSourceGetResult)
            {
                return;
            }

            value.Cancel();
            _logger.LogDebug("Canceled alone disconnect");
        }
    }

    public void CancelIdleDisconnect(LavalinkPlayer? player)
    {
        _logger.LogDebug("Canceling idle disconnect");
        if (player is GenericQueuedLavalinkPlayer zirconplayer)
        {
            var cancellationSourceGetResult = _disconnectTokens.TryGetValue(zirconplayer.VoiceChannelId ?? 0, out var value);

            if (value is null || value.IsCancellationRequested || !cancellationSourceGetResult)
            {
                return;
            }

            value.Cancel();
            _logger.LogDebug("Canceled idle Disconnect");
        }
    }
}
