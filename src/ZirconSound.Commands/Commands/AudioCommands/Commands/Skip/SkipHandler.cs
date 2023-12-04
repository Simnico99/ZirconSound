using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Skip;

public sealed class SkipHandler : ICommandHandler<SkipCommand>
{
    private readonly IAudioService _audioService;

    public SkipHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async ValueTask<Unit> Handle(SkipCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = await _audioService.GetPlayerAndSetContextAsync(command.Context.Guild.Id, command.Context);

        if (player?.CurrentTrack is null && player?.Queue.Count == 0)
        {
            embed.AddField("No tracks in queue:", "No tracks are actually in the queue or playing right now.");
            await command.Context.ReplyToLastCommandAsync(embed: embed.Build());
            return Unit.Value;
        }

        var track = player?.Queue.Any();
        if (track is not null && track.Value)
        {
            if (player?.Queue.Count > 0)
            {
                embed.AddField("Skipped now playing:", $"[{player?.Queue[0].Track!.Title}]({player?.Queue[0].Track!.Uri})");
                embed.EmbedSong(player!.Queue[0].Track!);
            }
        }

        if (track is null || track.Value == false)
        {
            if (player?.Queue.Count == 0)
            {
                await player.StopAsync(cancellationToken);
                embed.AddField("Skipped the last track:", "Bot will disconnect in 30 seconds.");
            }
        }

        if (player?.CurrentLoopingPlaylist?.Count > 0 || player?.Queue.Count > 0)
        {
            await player.SkipAsync(cancellationToken: cancellationToken);
        }

        if (player?.CurrentLoopingTrack is not null)
        {
            player.CurrentLoopingTrack = player.CurrentTrack;
            embed.AddField("Looping:", "This song will now be the one looping.");
        }

        if (player?.CurrentLoopingPlaylist is not null && player.Queue.Count <= 0)
        {
            if (player.CurrentTrack == player.CurrentLoopingPlaylist.Last())
            {
                embed.AddField("Restarting playist:", "Reached end of the looping playlist restarting it.");
                await player.Queue.ClearAsync(cancellationToken);
                await player.PlayAsync(player.CurrentLoopingPlaylist.First(), cancellationToken: cancellationToken);
                var playlist = player.CurrentLoopingPlaylist.Skip(1);

                foreach (var listTrack in playlist)
                {
                    await player.PlayAsync(listTrack, true, cancellationToken: cancellationToken);
                }
            }
        }

        if (player?.Queue.Count >= 1)
        {
            var queueLength = new EmbedFieldBuilder().WithName("Queue count:").WithValue($"{player.Queue.Count} tracks").WithIsInline(true);
            embed.AddField(queueLength);
        }

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build());

        return Unit.Value;
    }
}