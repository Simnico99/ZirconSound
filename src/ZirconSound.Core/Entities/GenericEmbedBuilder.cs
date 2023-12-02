using Discord.WebSocket;
using Discord;
using ZirconSound.Core.Enums;
using Lavalink4NET.Tracks;

namespace ZirconSound.Core.Entities;
public sealed class GenericEmbedBuilder : EmbedBuilder
{
    public GenericEmbedBuilder(BaseSocketClient socketClient)
    {
        var actualAuthor = new EmbedAuthorBuilder()
            .WithName(socketClient.CurrentUser.Username)
            .WithIconUrl(socketClient.CurrentUser.GetAvatarUrl());
        WithAuthor(actualAuthor);
    }

    public GenericEmbedBuilder(IUser user)
    {
        var actualAuthor = new EmbedAuthorBuilder()
            .WithName($"@{user.Username}{(user.Discriminator == "0000" ? "" : $"#{user.Discriminator}")}")
            .WithIconUrl(user.GetAvatarUrl());
        WithAuthor(actualAuthor);
    }

    private void ChangeType(GenericEmbedType embedType)
    {
        switch (embedType)
        {
            case GenericEmbedType.Info:
                WithColor(Discord.Color.DarkBlue);
                break;
            case GenericEmbedType.Warning:
                WithColor(Discord.Color.Orange);
                break;
            case GenericEmbedType.Error:
                WithColor(Discord.Color.DarkRed);
                break;
            case GenericEmbedType.Debug:
                WithColor(Discord.Color.DarkerGrey);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(embedType), embedType, null);
        }
    }

    public new Embed Build()
    {
        ChangeType(GenericEmbedType.Info);
        WithCurrentTimestamp();
        return base.Build();
    }

    public Embed Build(GenericEmbedType embedType)
    {
        ChangeType(embedType);
        WithCurrentTimestamp();
        return base.Build();
    }

    public void EmbedSong(LavalinkTrack lavalinkTrack)
    {
        var channel = new EmbedFieldBuilder().WithName("Channel").WithValue(lavalinkTrack.Author).WithIsInline(true);
        var duration = new EmbedFieldBuilder().WithName("Duration").WithValue(lavalinkTrack.Duration).WithIsInline(true);

        WithThumbnailUrl(lavalinkTrack.ArtworkUri?.ToString());
        AddField(channel);
        AddField(duration);
    }
}
