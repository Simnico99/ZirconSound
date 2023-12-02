using Mediator;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;

namespace ZirconSound.Application.Commands.GenericCommands.Requests.Help;

public sealed class HelpHandler : IRequestHandler<HelpRequest>
{


    public async ValueTask<Unit> Handle(HelpRequest request, CancellationToken cancellationToken)
    {

        var embed = EmbedHelpers.CreateGenericEmbedBuilder(request.Context);


        if (Environment.GetEnvironmentVariable("DOTNET_") == "development") 
        {
            embed.AddField("Cannot run in dev mode:", "This command can't run in dev mode!");
            await request.Context.ReplyToLastCommandAsync(embed: embed.Build(GenericEmbedType.Warning));
            return Unit.Value;
        }


        var actualCommands = await request.Context.Client.GetGlobalApplicationCommandsAsync();

        foreach (var commands in actualCommands)
        {
            embed.AddField(commands.Name.FirstCharToUpper(), commands.Description);
        }

        await request.Context.ReplyToLastCommandAsync(embed: embed.Build());

        return Unit.Value;
    }
}
