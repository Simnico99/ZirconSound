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

public sealed class LeaveHandler : ICommandHandler<LeaveCommand>
{
    private readonly IAudioService _audioService;

    public LeaveHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async ValueTask<Unit> Handle(LeaveCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = _audioService.GetPlayerAndSetContext(command.Context.Guild.Id, command.Context);

        player!.Queue.Clear();
        await player.DisconnectAsync();

        embed.AddField("Left:", "ZirconSound has left the current voice channel.");

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build());

        return Unit.Value;
    }
}