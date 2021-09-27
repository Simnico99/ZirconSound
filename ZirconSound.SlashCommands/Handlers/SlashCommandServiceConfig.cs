using Discord;
using Discord.Commands;

namespace ZirconSound.SlashCommands.Handlers
{
    public class SlashCommandServiceConfig
    {
        public RunMode DefaultRunMode = RunMode.Sync;
        public LogSeverity LogLevel = LogSeverity.Info;
    }
}