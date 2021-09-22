using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ZirconSound.DiscordHandlers;

namespace ZirconSound.Player
{
    public class PlayerService
    {
        private readonly ConcurrentDictionary<ulong?, CancellationTokenSource> _disconnectTokens;
        private readonly ConcurrentDictionary<ulong?, CancellationTokenSource> _alonedisconnectTokens;
        private readonly EmbedHandler _embedHandler;
        private readonly IAudioService _audioService;
        private readonly ILogger _logger;

        public PlayerService(EmbedHandler embedHandler, IAudioService audioService, ILogger<PlayerService> logger)
        {
            _disconnectTokens = new();
            _alonedisconnectTokens = new();
            _audioService = audioService;
            _logger = logger;
            _embedHandler = embedHandler;

            _audioService.TrackEnd += AudioService_TrackEnd;
            _audioService.TrackStuck += AudioService_TrackStuck;
            _audioService.TrackStarted += AudioService_TrackStarted;
            _audioService.TrackException += AudioService_TrackException;
        }

        private async Task AudioService_TrackException(object sender, TrackExceptionEventArgs eventArgs)
        {
            var player = (QueuedLavalinkPlayer)eventArgs.Player;

            _logger.LogWarning($"Track {eventArgs.Player.CurrentTrack.Title} threw an exception. Please check Lavalink console/logs.");
            var embed = _embedHandler.Create();
            embed.AddField("Error:", $"{player.CurrentTrack.Title} has been skipped after throwing an exception.");
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
            _logger.LogWarning($"Track {eventArgs.Player.CurrentTrack.Title} got stuck. Please check Lavalink console/logs.");
            var player = (QueuedLavalinkPlayer)eventArgs.Player;

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

        private async Task AudioService_TrackStarted(object sender, TrackStartedEventArgs eventArgs)
        {
            await CancelDisconnectAsync(eventArgs.Player);
        }

        private async Task AudioService_TrackEnd(object sender, TrackEndEventArgs eventArgs)
        {
            var player = (QueuedLavalinkPlayer)eventArgs.Player;

            if (player.Queue.IsEmpty)
            {
                await InitiateDisconnectAsync(eventArgs.Player, TimeSpan.FromSeconds(40));
            }
        }

        public async Task CancelDisconnectAsync(LavalinkPlayer player)
        {
            _logger.LogDebug("Canceling Disconnect");
            if (!_disconnectTokens.TryGetValue(player.VoiceChannelId, out var value))
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
            if (!_alonedisconnectTokens.TryGetValue(player.VoiceChannelId, out var value))
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

        public async Task InitiateDisconnectAsync(LavalinkPlayer player, TimeSpan timeSpan)
        {
            _logger.LogDebug("Disconnecting");
            if (!_disconnectTokens.TryGetValue(player.VoiceChannelId, out var value))
            {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannelId, value);
            }
            else if (value.IsCancellationRequested)
            {
                _disconnectTokens.TryUpdate(player.VoiceChannelId, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannelId];
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
            if (!_alonedisconnectTokens.TryGetValue(player.VoiceChannelId, out var value))
            {
                value = new CancellationTokenSource();
                _alonedisconnectTokens.TryAdd(player.VoiceChannelId, value);
            }
            else if (value.IsCancellationRequested)
            {
                _alonedisconnectTokens.TryUpdate(player.VoiceChannelId, new CancellationTokenSource(), value);
                value = _alonedisconnectTokens[player.VoiceChannelId];
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
