using Lavalink4NET;
using Lavalink4NET.Rest;
using Mediator;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;
using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.SkipCommand;

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
        var player = _audioService.GetPlayerAndSetContext(command.Context.Guild.Id, command.Context);
        var button = new ComponentBuilder().WithButton("Resume track", "track-button-resume", ButtonStyle.Primary).Build();

        await player!.PauseAsync();
        LavalinkPlayerHelper.StartIdleDisconnectTimer(player, TimeSpan.FromMinutes(1));

        embed.AddField("Paused:", $"{player.CurrentTrack?.Title} has been paused.");

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build(), component: button);

        return Unit.Value;
    }
}