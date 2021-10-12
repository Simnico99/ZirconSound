namespace ZirconSound.Handlers;

internal static class MessageComponentHandler
{
    public static async Task Invoke(IDiscordClient client, SocketMessageComponent componentInteraction, InteractionsService commandService)
    {
        var componentContext = new MessageComponentContext(client, componentInteraction) { ModifyOriginalMessage = true };
        await commandService.Invoke(componentInteraction, componentContext);
    }

    public static async Task Executed(Optional<SocketMessageComponent> commandInfo, IInteractionContext commandContext, IResult result)
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
