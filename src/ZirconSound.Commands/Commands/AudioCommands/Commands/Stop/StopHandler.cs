using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.SkipCommand;

public sealed class StopHandler : ICommandHandler<StopCommand>
{
    private readonly IAudioService _audioService;

    public StopHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async ValueTask<Unit> Handle(StopCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = _audioService.GetPlayerAndSetContext(command.Context.Guild.Id, command.Context);

        player!.Queue.Clear();
        await player.StopAsync();

        embed.AddField("Stopped and cleared queue:", $"Track has been stopped and queue has been cleared");

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build());

        return Unit.Value;
    }
}