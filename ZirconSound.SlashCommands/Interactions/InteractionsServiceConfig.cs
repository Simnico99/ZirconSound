using Discord;
using Discord.Commands;

namespace ZirconSound.ApplicationCommands.Interactions
{
    public class InteractionsServiceConfig
    {
        public RunMode DefaultRunMode = RunMode.Sync;
        public LogSeverity LogLevel = LogSeverity.Info;
    }
}