using Mediator;
using ZirconSound.Core.Extensions;

namespace ZirconSound.Application.Commands.GenericCommands.Requests.Ping;

public sealed class PingHandler : IRequestHandler<PingRequest>
{
    public async ValueTask<Unit> Handle(PingRequest request, CancellationToken cancellationToken)
    {
        await request.Context.ReplyToLastCommandAsync("PONG!");

        return Unit.Value;
    }
}
