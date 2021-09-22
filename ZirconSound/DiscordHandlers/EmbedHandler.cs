using Discord.Commands;
using Discord.WebSocket;

namespace ZirconSound.DiscordHandlers
{
    public class EmbedHandler
    {
        public DiscordSocketClient Client;

        public EmbedHandler(DiscordSocketClient client)
        {
            Client = client;
        }

        public ZirconEmbed Create()
        {
            var embed = new ZirconEmbed(Client);

            return embed;
        }

        public ZirconEmbed Create(SocketCommandContext socketCommand)
        {
            var embed = new ZirconEmbed(socketCommand.User);

            return embed;
        }
    }
}
