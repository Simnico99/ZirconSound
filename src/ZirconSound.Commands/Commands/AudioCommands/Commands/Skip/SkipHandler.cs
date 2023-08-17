using Lavalink4NET;
using Lavalink4NET.Rest;
using Mediator;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;
using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.SkipCommand;

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
        var player = _audioService.GetPlayerAndSetContext(command.Context.Guild.Id, command.Context);


        player!.SkippedOnPurpose = true;

        if (player?.CurrentTrack is null && player?.Queue.Count == 0)
        {
            embed.AddField("No tracks in queue:", "No tracks are actually in the queue or playing right now.");
            await command.Context.ReplyToLastCommandAsync(embed: embed.Build());
            return Unit.Value;
        }

        var track = player?.Queue.FirstOrDefault();
        if (track is not null)
        {
            if (player?.Queue.Count > 0)
            {
                embed.AddField("Skipped now playing:", $"[{player?.Queue.First().Title}]({player?.Queue.First().Uri})");
                embed.EmbedSong(player?.Queue.First()!);
            }
        }

        if (track is null)
        {
            if (player?.Queue.Count == 0)
            {
                await player.StopAsync();
                embed.AddField("Skipped the last track:", "Bot will disconnect in 30 seconds.");
            }
        }

        if (player?.CurrentLoopingPlaylist?.Count > 0 || player?.Queue.Count > 0)
        {
            await player.SkipAsync();
        }

        if (player?.CurrentLoopingPlaylist is not null && player.Queue.Count <= 0)
        {
            if (player.CurrentTrack == player.CurrentLoopingPlaylist.Last())
            {
                embed.AddField("Restarting playist:", "Reached end of the looping playlist restarting it.");
                player.Queue.Clear();
                await player.PlayAsync(player.CurrentLoopingPlaylist.First());
                var playlist = player.CurrentLoopingPlaylist.Skip(1);

                foreach (var listTrack in playlist)
                {
                    player.Queue.Add(listTrack);
                }
            }
        }

        if (player?.Queue.Count >= 1)
        {
            var queueLength = new EmbedFieldBuilder().WithName("Queue count").WithValue($"{player.Queue.Count} tracks").WithIsInline(true);
            embed.AddField(queueLength);
        }

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build());

        return Unit.Value;
    }
}