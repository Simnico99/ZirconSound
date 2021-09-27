using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ZirconSound.Logger
{
    public class ConsoleLoggerConfiguration
    {
        public static int EventId => 0;

        public Dictionary<LogLevel, ConsoleColor> LogLevels { get; } = new()
        {
            [LogLevel.Information] = ConsoleColor.DarkGreen,
            [LogLevel.Warning] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Black,
            [LogLevel.Debug] = ConsoleColor.DarkCyan
        };

        public Dictionary<LogLevel, ConsoleColor> LogBackground { get; } = new()
        {
            [LogLevel.Information] = ConsoleColor.Black,
            [LogLevel.Warning] = ConsoleColor.Black,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Debug] = ConsoleColor.Black
        };

        public Dictionary<LogLevel, string> LogShort { get; } = new()
        {
            [LogLevel.Information] = "Info",
            [LogLevel.Warning] = "Warn",
            [LogLevel.Error] = "Fail",
            [LogLevel.Debug] = "Dbg"
        };
    }
}