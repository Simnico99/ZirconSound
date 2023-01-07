using Discord;
using Mediator;

namespace ZirconSound.Application.Commands.GenericCommands.Requests.Ping;

public sealed record PingRequest(IInteractionContext Context) : IRequest;
