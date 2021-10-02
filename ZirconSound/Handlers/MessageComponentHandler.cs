using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using ZirconSound.ApplicationCommands.Interactions;
using ZirconSound.ApplicationCommands.MessageComponents;
using ZirconSound.Embeds;
using ZirconSound.Enum;

namespace ZirconSound.Handlers
{
    internal static class MessageComponentHandler
    {
        public static async Task Invoke(IDiscordClient client, SocketMessageComponent componentInteraction, InteractionsService commandService)
        {
            var componentContext = new MessageComponentContext(client, componentInteraction);
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
}
