using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;

namespace ZirconSound.Services
{
    public class AudioService : IHostedService
    {
        private ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        public LavaNode LavaNode { get; }

        public AudioService(LavaNode lavaNode) 
        {
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
            LavaNode = lavaNode;

            LavaNode.OnTrackStarted += OnTrackStarted;
            LavaNode.OnTrackEnded += OnTrackEnded;
            LavaNode.OnTrackException += OnTrackException;
            LavaNode.OnTrackStuck += OnTrackStuck;

        }


        public async Task CancelDisconnect(LavaPlayer player) 
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value))
            {
                value = new CancellationTokenSource();
            }
            else if (value.IsCancellationRequested)
            {
                return;
            }

            await Task.Run(() => value.Cancel(true));
        }

        private async Task OnTrackException(TrackExceptionEventArgs arg)
        {
            Console.WriteLine($"Track {arg.Track.Title} threw an exception. Please check Lavalink console/logs.");
            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel?.SendMessageAsync(
                $"{arg.Track.Title} has been re-added to queue after throwing an exception.");
        }

        private async Task OnTrackStuck(TrackStuckEventArgs arg)
        {
            Console.WriteLine(
                $"Track {arg.Track.Title} got stuck for {arg.Threshold}ms. Please check Lavalink console/logs.");
            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel?.SendMessageAsync(
                $"{arg.Track.Title} has been re-added to queue after getting stuck.");
        }


        private async Task OnTrackStarted(TrackStartEventArgs arg)
        {
            if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out var value))
            {
                return;
            }

            if (value.IsCancellationRequested)
            {
                return;
            }
            await Task.Run(() => value.Cancel(true));
        }

        public async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeSpan)
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value))
            {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
            }
            else if (value.IsCancellationRequested)
            {
                _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannel.Id];
            }

            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await LavaNode.LeaveAsync(player.VoiceChannel);
            await player.TextChannel.SendMessageAsync("Invite me again for nice music! (!join)");
        }


        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (args.Reason != Victoria.Enums.TrackEndReason.Finished)
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                //await player.TextChannel.SendMessageAsync("Queue completed!");
                await InitiateDisconnectAsync(args.Player, TimeSpan.FromSeconds(120));
                return;
            }

            if (queueable is not LavaTrack track)
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            //await args.Player.TextChannel.SendMessageAsync($"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return null;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return null;
        }

    }
}
