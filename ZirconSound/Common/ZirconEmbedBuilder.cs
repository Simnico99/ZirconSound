using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZirconSound.Common
{
    class ZirconEmbedBuilder : EmbedBuilder
    {
        private readonly DiscordSocketClient _client;

        public ZirconEmbedBuilder(DiscordSocketClient socketClient)
        {
            _client = socketClient;
        }

        public void ChangeType(ZirconEmbedType embedType) 
        {
            if (embedType == ZirconEmbedType.Info)
            {
                this.WithColor(Discord.Color.DarkBlue);
            }
            else if (embedType == ZirconEmbedType.Warning)
            {
                this.WithColor(Discord.Color.Orange);
            }
            else if (embedType == ZirconEmbedType.Error)
            {
                this.WithColor(Discord.Color.DarkRed);
            }
        }

        public ZirconEmbedBuilder(ZirconEmbedType embedType = ZirconEmbedType.Info)
        {
            ChangeType(embedType);
            this.WithTimestamp(DateTime.Now);
        }
    }
}
