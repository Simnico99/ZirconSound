using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ZirconSound.SlashCommands
{
    public class SlashCommandContext : ICommandContext
    {
        public IDiscordClient Client { get; }

        public IGuild Guild { get; }

        public IMessageChannel Channel { get; }

        public IUser User { get; }

        public IUserMessage Message { get; }

        public SocketSlashCommand Command { get; }

        SlashCommandContext() { }

        public SlashCommandContext(IDiscordClient client, SocketSlashCommand command)
        {
            Client = client;
            var chnl = command.Channel as SocketGuildChannel;
            Guild = chnl.Guild;
            User = command.User;
            Channel = command.Channel;
            Command = command;
        }
    }
}
