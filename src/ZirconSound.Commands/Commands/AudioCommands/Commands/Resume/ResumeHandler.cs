using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.SkipCommand;

public sealed class ResumeHandler : ICommandHandler<ResumeCommand>
{
    private readonly IAudioService _audioService;

    public ResumeHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async ValueTask<Unit> Handle(ResumeCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = await _audioService.GetPlayerAndSetContextAsync(command.Context.Guild.Id, command.Context);
        var button = new ComponentBuilder().WithButton("Pause track", "track-button-pause", ButtonStyle.Danger).Build();

        await player!.ResumeAsync(cancellationToken);

        embed.AddField("Resumed:", $"{player.CurrentTrack?.Title} has been resumed.");

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build(), component: button);

        return Unit.Value;
    }
}