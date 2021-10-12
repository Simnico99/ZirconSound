namespace ZirconSound.Handlers;

public static class SlashCommandHandler
{
    public static async Task Invoke(IDiscordClient client, SocketSlashCommand commandInteraction, InteractionsService commandService)
    {
        var commandContext = new SlashCommandContext(client, commandInteraction);
        await commandService.Invoke(commandInteraction, commandContext);
    }

    public static async Task Executed(Optional<SocketSlashCommand> commandInfo, IInteractionContext commandContext, IResult result)
    {
        if (result.IsSuccess)
        {
            return;
        }

        var embed = EmbedHandler.Create(commandContext);
        embed.AddField("Error:", result.ErrorReason);
        await commandInfo.Value.FollowupAsync(embed: embed.BuildSync(ZirconEmbedType.Error));
    }
}
