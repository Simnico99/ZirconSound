namespace ZirconSound.Embeds;

public class ZirconEmbed : EmbedBuilder
{
    public ZirconEmbed(BaseSocketClient socketClient)
    {
        var actualAuthor = new EmbedAuthorBuilder()
            .WithName(socketClient.CurrentUser.Username)
            .WithIconUrl(socketClient.CurrentUser.GetAvatarUrl());
        WithAuthor(actualAuthor);
    }

    public ZirconEmbed(IUser user)
    {
        var actualAuthor = new EmbedAuthorBuilder()
            .WithName($"@{user.Username}#{user.Discriminator}")
            .WithIconUrl(user.GetAvatarUrl());
        WithAuthor(actualAuthor);
    }

    private void ChangeType(ZirconEmbedType embedType)
    {
        switch (embedType)
        {
            case ZirconEmbedType.Info:
                WithColor(Discord.Color.DarkBlue);
                break;
            case ZirconEmbedType.Warning:
                WithColor(Discord.Color.Orange);
                break;
            case ZirconEmbedType.Error:
                WithColor(Discord.Color.DarkRed);
                break;
            case ZirconEmbedType.Debug:
                WithColor(Discord.Color.DarkerGrey);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(embedType), embedType, null);
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
