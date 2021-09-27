using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ZirconSound.SlashCommands.Handlers
{
    public class SlashCommandContext : ICommandContext
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
            Command = command;
        }

        public SocketSlashCommand Command { get; }
        public IDiscordClient Client { get; }

        public IGuild Guild { get; }

        public IMessageChannel Channel { get; }

        public IUser User { get; }

        public IUserMessage Message { get; }
    }
}