using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Clear;

public sealed class ClearHandler : ICommandHandler<ClearCommand>
{
    private readonly IAudioService _audioService;

    public ClearHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async ValueTask<Unit> Handle(ClearCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = await _audioService.GetPlayerAndSetContextAsync(command.Context.Guild.Id, command.Context);

        await player!.Queue.ClearAsync(cancellationToken);

        embed.AddField("Cleared:", "The queue as been cleared successfully");

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build());

        return Unit.Value;
    }
}