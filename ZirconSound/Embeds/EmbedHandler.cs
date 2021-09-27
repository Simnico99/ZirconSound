using Discord.Commands;
using Discord.WebSocket;

namespace ZirconSound.Embeds
{
    public class EmbedHandler
    {
        private readonly DiscordSocketClient _client;

        public EmbedHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public ZirconEmbed Create()
        {
            var embed = new ZirconEmbed(_client);

            return embed;
        }

        public static ZirconEmbed Create(ICommandContext socketCommand)
        {
            var embed = new ZirconEmbed(socketCommand.User);

            return embed;
        }
    }
}