using Discord.WebSocket;
using Discord;
using ZirconSound.Core.Entities;

namespace ZirconSound.Core.Helpers;
public sealed class EmbedHelpers
{
    private readonly DiscordSocketClient _client;

    public EmbedHelpers(DiscordSocketClient client) => _client = client;

    public GenericEmbedBuilder CreateGenericEmbedBuilder()
    {
        var embed = new GenericEmbedBuilder(_client);

        return embed;
    }

    public static GenericEmbedBuilder CreateGenericEmbedBuilder(IInteractionContext socketCommand)
    {
        var embed = new GenericEmbedBuilder(socketCommand.User);

        return embed;
    }
}
