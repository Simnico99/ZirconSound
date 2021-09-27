using Discord;
using Discord.WebSocket;
using ZirconSound.Enum;

namespace ZirconSound.Embeds
{
    public class ZirconEmbed : EmbedBuilder
    {
        private readonly DiscordSocketClient _client;
        private readonly EmbedAuthorBuilder _actualAuthor;

        public ZirconEmbed(DiscordSocketClient socketClient)
        {
            _client = socketClient;
            _actualAuthor = new EmbedAuthorBuilder()
            .WithName(_client.CurrentUser.Username)
            .WithIconUrl(_client.CurrentUser.GetAvatarUrl());
            WithAuthor(_actualAuthor);
        }

        public ZirconEmbed(IUser user)
        {
            _actualAuthor = new EmbedAuthorBuilder()
            .WithName($"@{user.Username}#{user.Discriminator}")
            .WithIconUrl(user.GetAvatarUrl());
            WithAuthor(_actualAuthor);
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
            else if (embedType == ZirconEmbedType.Debug)
            {
                WithColor(Discord.Color.DarkerGrey);
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
