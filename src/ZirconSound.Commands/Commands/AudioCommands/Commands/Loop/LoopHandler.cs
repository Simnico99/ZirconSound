using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Loop;

public sealed class LoopHandler : ICommandHandler<LoopCommand>
{
    private readonly IAudioService _audioService;

    public LoopHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async ValueTask<Unit> Handle(LoopCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = await _audioService.GetPlayerAndSetContextAsync(command.Context.Guild.Id, command.Context);
        var button = new ComponentBuilder().WithButton("Stop Loop", "loop-button-stop", ButtonStyle.Danger).Build();

        player!.CurrentLoopingPlaylist = null;
        player.CurrentLoopingTrack = null;

        switch (command.LoopType)
        {
            case LoopType.Cancel:
                embed.AddField("Looping stopped:", "Stopped the loop.");
                await command.Context.ReplyToLastCommandAsync(embed: embed.Build());
                button = new ComponentBuilder()
                    .WithButton("Loop Track", "loop-button-track", ButtonStyle.Secondary)
                    .WithButton("Loop Playlist", "loop-button-playlist", ButtonStyle.Secondary)
                    .Build();
                break;
            case LoopType.Track:
                embed.AddField("Looping:", "The current track.");
                player.CurrentLoopingTrack = player.CurrentTrack;
                break;
            case LoopType.Playlist:
                embed.AddField("Looping:", "The current playlist.");
                player.CurrentLoopingPlaylist = player.Queue.Select(x => x.Track).Prepend(player.CurrentTrack).ToList()!;
                break;
        }

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build(), component: button);

        return Unit.Value;
    }
}
