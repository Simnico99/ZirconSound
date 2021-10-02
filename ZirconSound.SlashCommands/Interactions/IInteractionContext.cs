using Discord;
using Discord.WebSocket;

namespace ZirconSound.ApplicationCommands.Interactions
{
    public interface IInteractionContext
    {
        SocketInteraction Interaction { get; }

        IDiscordClient Client { get; }

        IGuild Guild { get; }

        IMessageChannel Channel { get; }

        IUser User { get; }

        bool ModifyOriginalMessage { get; set; }
    }
}
