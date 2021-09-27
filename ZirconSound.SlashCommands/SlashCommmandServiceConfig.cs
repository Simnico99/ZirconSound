using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZirconSound.SlashCommands
{
    public class SlashCommmandServiceConfig
    {
        public RunMode DefaultRunMode = RunMode.Sync;
        public LogSeverity LogLevel = LogSeverity.Info;
    }
}
