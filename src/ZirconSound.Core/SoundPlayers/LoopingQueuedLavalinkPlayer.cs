using Discord;
using Lavalink4NET.InactivityTracking.Players;
using Lavalink4NET.InactivityTracking.Trackers;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Tracks;

namespace ZirconSound.Core.SoundPlayers;
public sealed class LoopingQueuedLavalinkPlayer : QueuedLavalinkPlayer, IInactivityPlayerListener
{
    public LoopingQueuedLavalinkPlayer(IPlayerProperties<QueuedLavalinkPlayer, QueuedLavalinkPlayerOptions> properties) : base(properties)
    {
    }

    public LavalinkTrack? CurrentLoopingTrack { get; set; }
    public List<LavalinkTrack>? CurrentLoopingPlaylist { get; set; }
    public IInteractionContext? Context { get; set; }

    public ValueTask NotifyPlayerActiveAsync(PlayerTrackingState trackingState, CancellationToken cancellationToken = default)
    {
        return default;
    }

    public async ValueTask NotifyPlayerInactiveAsync(PlayerTrackingState trackingState, CancellationToken cancellationToken = default)
    {
        await DisconnectAsync(cancellationToken);
    }

    public ValueTask NotifyPlayerTrackedAsync(PlayerTrackingState trackingState, CancellationToken cancellationToken = default)
    {
        return default;
    }

    protected async override ValueTask NotifyTrackEndedAsync(ITrackQueueItem queueItem, TrackEndReason endReason, CancellationToken cancellationToken = default)
    {
        await base.NotifyTrackEndedAsync(queueItem, endReason, cancellationToken);

        if (endReason == TrackEndReason.Finished)
        {
            if (CurrentLoopingTrack is not null)
            {
                await PlayAsync(CurrentLoopingTrack, false, cancellationToken: cancellationToken);
            }

            if (CurrentLoopingPlaylist is not null && CurrentLoopingPlaylist.Count > 0)
            {
                if (queueItem.Track == CurrentLoopingPlaylist.Last())
                {
                    await Queue.ClearAsync(cancellationToken);
                    await StopAsync(cancellationToken);
                    foreach (var track in CurrentLoopingPlaylist)
                    {
                        await PlayAsync(track, true, cancellationToken: cancellationToken);
                    }
                }
            }
        }
    }
}
