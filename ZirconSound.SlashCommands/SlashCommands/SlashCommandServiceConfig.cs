﻿using Discord;
using Discord.Commands;

namespace ZirconSound.ApplicationCommands.SlashCommands
{
    public class SlashCommandServiceConfig
    {
        public RunMode DefaultRunMode = RunMode.Sync;
        public LogSeverity LogLevel = LogSeverity.Info;
    }
}