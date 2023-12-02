using Discord;

namespace ZirconSound.Core.Extensions;
public static class SocketInteractionExtensions
{
    public static async Task ReplyToLastCommandAsync(this IInteractionContext interactionContext, string? text = null, Embed[]? embeds = null, AllowedMentions? allowedMentions = null, MessageComponent? component = null, Embed? embed = null, bool ephemeral = false)
    {
        component ??= new ComponentBuilder().Build();

        if (!ephemeral)
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
}
