using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Pause;

public sealed class PauseHandler : ICommandHandler<PauseCommand>
{
    private readonly IAudioService _audioService;

    public PauseHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async ValueTask<Unit> Handle(PauseCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = await _audioService.GetPlayerAndSetContextAsync(command.Context.Guild.Id, command.Context);
        var button = new ComponentBuilder().WithButton("Resume track", "track-button-resume", ButtonStyle.Primary).Build();

        await player!.PauseAsync(cancellationToken);

        embed.AddField("Paused:", $"{player.CurrentTrack?.Title} has been paused.");

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build(), component: button);

        return Unit.Value;
    }
}