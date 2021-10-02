using Discord;
using Discord.WebSocket;
using ZirconSound.ApplicationCommands.Interactions;

namespace ZirconSound.ApplicationCommands.SlashCommands
{
    public class SlashCommandContext : IInteractionContext
    {
        private SlashCommandContext()
        {
        }

        public SlashCommandContext(IDiscordClient client, SocketSlashCommand command)
        {
            Client = client;
            var chanel = command.Channel as SocketGuildChannel;
            Guild = chanel?.Guild;
            User = command.User;
            Channel = command.Channel;
            Interaction = command;
        }


        public IDiscordClient Client { get; }

        public IGuild Guild { get; }

        public IMessageChannel Channel { get; }

        public IUser User { get; }

        public SocketInteraction Interaction { get; }
    }
}