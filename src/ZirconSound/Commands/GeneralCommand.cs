using Discord.Interactions;

namespace ZirconSound.Commands;

public class GeneralCommand : InteractionModuleBase<IInteractionContext>
{
    private readonly DiscordSocketClient _client;

    public GeneralCommand(DiscordSocketClient client)
    {
        _client = client;
    }

    [SlashCommand("ping", "Ping the bot.")]
    public async Task Ping() => await Context.ReplyToCommandAsync("PONG!");

    [SlashCommand("help", "Show the commands you can execute.")]
    public async Task Help()
    {
        var embed = EmbedHandler.Create(Context);
        var actualCommands = await _client.GetGlobalApplicationCommandsAsync();

        foreach (var commands in actualCommands)
        {
            embed.AddField(commands.Name.FirstCharToUpper(), commands.Description);
        }

        await Context.ReplyToCommandAsync(embed: embed.BuildSync());
    }
}
