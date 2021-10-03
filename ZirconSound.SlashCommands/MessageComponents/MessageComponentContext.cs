using Discord;
using Discord.WebSocket;
using ZirconSound.ApplicationCommands.Interactions;

namespace ZirconSound.ApplicationCommands.MessageComponents
{
    public class MessageComponentContext : IInteractionContext
    {
        private MessageComponentContext()
        {
        }

        public MessageComponentContext(IDiscordClient client, SocketMessageComponent component)
        {
            Client = client;
            var chanel = component.Channel as SocketGuildChannel;
            Guild = chanel?.Guild;
            User = component.User;
            Channel = component.Channel;
            Interaction = component;
        }

        public IDiscordClient Client { get; }

        public IGuild Guild { get; }

        public IMessageChannel Channel { get; }

        public IUser User { get; }

        public SocketInteraction Interaction { get; }

        public bool ModifyOriginalMessage { get; set; }
    }
}