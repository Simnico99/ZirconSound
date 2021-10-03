using Discord;
using System.Threading.Tasks;
using ZirconSound.ApplicationCommands.Interactions;

namespace ZirconSound.Extensions
{
    internal static class SocketInteractionExtensions
    {
        public static async Task ReplyToCommandAsync(this IInteractionContext interactionContext, string text = null, Embed[] embeds = null, bool isTts = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent component = null, Embed embed = null)
        {
            if (interactionContext.ModifyOriginalMessage)
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
            else
            {
                await interactionContext.Interaction.FollowupAsync(text, embeds, isTts, ephemeral, allowedMentions, options, component, embed);
            }
        }
    }
}
