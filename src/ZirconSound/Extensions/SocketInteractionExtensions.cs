namespace ZirconSound.Extensions;

internal static class SocketInteractionExtensions
{
    public static async Task ReplyToCommandAsync(this IInteractionContext interactionContext, string text = null, Embed[] embeds = null, AllowedMentions allowedMentions = null, MessageComponent component = null, Embed embed = null, bool ephemeral = false)
    {
        await interactionContext.Interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Content = text;
            msg.Embeds = embeds;
            msg.AllowedMentions = allowedMentions;
            msg.Embed = embed;
            msg.Components = component;
        });
    }
}
