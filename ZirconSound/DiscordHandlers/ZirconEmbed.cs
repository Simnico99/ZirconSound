using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lavalink4NET;
using ZirconSound.Enum;

namespace ZirconSound.DiscordHandlers
{
    public class ZirconEmbed : EmbedBuilder
    {
        private readonly DiscordSocketClient _client;
        private readonly EmbedAuthorBuilder botAuthor;

        public ZirconEmbed(DiscordSocketClient socketClient)
        {
            _client = socketClient;
            botAuthor = new EmbedAuthorBuilder()
            .WithName(_client.CurrentUser.Username)
            .WithIconUrl(_client.CurrentUser.GetAvatarUrl());
            WithAuthor(botAuthor);
        }

        private void ChangeType(ZirconEmbedType embedType)
        {
            if (embedType == ZirconEmbedType.Info)
            {
                WithColor(Discord.Color.DarkBlue);
            }
            else if (embedType == ZirconEmbedType.Warning)
            {
                WithColor(Discord.Color.Orange);
            }
            else if (embedType == ZirconEmbedType.Error)
            {
                WithColor(Discord.Color.DarkRed);
            }
        }

        public Embed BuildSync() 
        {
            ChangeType(ZirconEmbedType.Info);
            WithCurrentTimestamp();
            return Build();
        }

        public Embed BuildSync(ZirconEmbedType embedType)
        {
            ChangeType(embedType);
            WithCurrentTimestamp();
            return Build();
        }
    }
}
