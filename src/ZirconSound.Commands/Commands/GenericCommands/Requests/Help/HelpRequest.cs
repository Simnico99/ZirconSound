using Discord;
using Mediator;

namespace ZirconSound.Application.Commands.GenericCommands.Requests.Help;

public sealed record HelpRequest(IInteractionContext Context) : IRequest;
