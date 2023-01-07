using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Mediator;
using ZirconSound.Application.Commands.GenericCommands.Requests.Help;
using ZirconSound.Application.Commands.GenericCommands.Requests.Ping;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;

namespace ZirconSound.Application.Commands.GenericCommands;
public sealed class GenericCommands : InteractionModuleBase<IInteractionContext>
{
    private readonly IMediator _mediator;

    public GenericCommands(IMediator mediator)
    {
        _mediator = mediator;
    }

    [SlashCommand("ping", "Ping the bot.")]
    public async Task Ping() => await _mediator.Send(new PingRequest(Context));

    [SlashCommand("help", "Show the commands you can execute.")]
    public async Task Help() => await _mediator.Send(new HelpRequest(Context));
}
