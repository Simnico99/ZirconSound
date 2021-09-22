using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZirconSound.Services;

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
    }
}
