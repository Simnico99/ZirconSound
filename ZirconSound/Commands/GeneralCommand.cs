namespace ZirconSound.Commands;

public class GeneralCommand : InteractionModule<IInteractionContext>
{
    private readonly InteractionsService _interactionsService;

    public GeneralCommand(InteractionsService slashInteractions) => _interactionsService = slashInteractions;


    [SlashCommand("ping", "Ping the bot.")]
    public async Task Ping() => await Context.ReplyToCommandAsync("PONG!");

    [SlashCommand("help", "Show the commands you can execute.")]
    public async Task Help()
    {
        var embed = EmbedHandler.Create(Context);

        foreach (var commands in _interactionsService.SlashCommands)
        {
            embed.AddField(commands.Interaction.Name.FirstCharToUpper(), commands.Interaction.Description);
        }

        await Context.ReplyToCommandAsync(embed: embed.BuildSync());
    }
}
