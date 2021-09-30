using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ZirconSound.ApplicationCommands.Interactions;
using ZirconSound.ApplicationCommands.SlashCommands;
using ZirconSound.Embeds;
using ZirconSound.Enum;

namespace ZirconSound.Handlers
{
    public static class SlashCommandHandler
    {
        public static async Task Invoke(IDiscordClient client, SocketSlashCommand commandInteraction, InteractionsService commandService)
        {
            var commandContext = new SlashCommandContext(client, commandInteraction);
            await commandService.InvokeSlashCommand(commandInteraction, commandContext);
        }

        public static async Task InteractionsExecuted(Optional<SocketSlashCommand> commandInfo, ICommandContext commandContext, IResult result)
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
