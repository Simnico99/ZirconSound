using Lavalink4NET;
using Lavalink4NET.Rest;
using Mediator;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;
using ZirconSound.Application.Services;
using Discord;
using Lavalink4NET.Player;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.PlayCommand;

public sealed class LoopHandler : ICommandHandler<LoopCommand>
{
    private readonly IAudioService _audioService;
    private readonly ICustomPlayerService _customPlayerService;

    public LoopHandler(IAudioService audioService, ICustomPlayerService customPlayerService)
    {
        _audioService = audioService;
        _customPlayerService = customPlayerService;
    }

    public async ValueTask<Unit> Handle(LoopCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = _audioService.GetPlayerAndSetContext(command.Context.Guild.Id, command.Context);
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
                player.CurrentLoopingPlaylist = player.Queue.Prepend(player.CurrentTrack).ToList()!;
                break;
        }

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build(), component: button);

        return Unit.Value;
    }
}
