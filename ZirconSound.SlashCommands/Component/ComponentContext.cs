using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ZirconSound.ApplicationCommands.Component
{
    public class ComponentContext : ICommandContext
    {
        private ComponentContext()
        {
        }

        public ComponentContext(IDiscordClient client, SocketMessageComponent component)
        {
            Client = client;
            var chanel = component.Channel as SocketGuildChannel;
            Guild = chanel?.Guild;
            User = component.User;
            Channel = component.Channel;
            Component = component;
            Message = component.Message;
        }

        public SocketMessageComponent Component { get; }

        public IDiscordClient Client { get; }

        public IGuild Guild { get; }

        public IMessageChannel Channel { get; }

        public IUser User { get; }

        public IUserMessage Message { get; }
    }
}