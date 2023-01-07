using Lavalink4NET;
using Lavalink4NET.Rest;
using Mediator;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;
using ZirconSound.Application.Services;
using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.SkipCommand;

public sealed class ResumeHandler : ICommandHandler<ResumeCommand>
{
    private readonly IAudioService _audioService;
    private readonly ICustomPlayerService _customPlayerService;

    public ResumeHandler(IAudioService audioService, ICustomPlayerService customPlayerService)
    {
        _audioService = audioService;
        _customPlayerService = customPlayerService;
    }

    public async ValueTask<Unit> Handle(ResumeCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = _audioService.GetPlayerAndSetContext(command.Context.Guild.Id, command.Context);
        var button = new ComponentBuilder().WithButton("Pause track", "track-button-pause", ButtonStyle.Danger).Build();

        await player!.ResumeAsync();
        _customPlayerService.CancelIdleDisconnect(player);

        embed.AddField("Resumed:", $"{player.CurrentTrack?.Title} has been resumed.");

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build(), component: button);

        return Unit.Value;
    }
}