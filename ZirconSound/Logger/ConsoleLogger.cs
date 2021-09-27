using Microsoft.Extensions.Logging;
using System;

namespace ZirconSound.Logger
{
    public class ConsoleLogger : ILogger
    {
        private readonly Func<ConsoleLoggerConfiguration> _getCurrentConfig;
        private readonly string _name;

        public ConsoleLogger(
            string name,
            Func<ConsoleLoggerConfiguration> getCurrentConfig) => (_name, _getCurrentConfig) = (name, getCurrentConfig);

        public IDisposable BeginScope<TState>(TState state) => default;

        public bool IsEnabled(LogLevel logLevel) => _getCurrentConfig().LogLevels.ContainsKey(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var config = _getCurrentConfig();
            if (ConsoleLoggerConfiguration.EventId == 0 || ConsoleLoggerConfiguration.EventId == eventId.Id)
            {
                if (!string.IsNullOrEmpty(formatter(state, exception)) && !string.IsNullOrEmpty(eventId.Id.ToString()) && _name != null)
                {
                    try
                    {
                        Console.ForegroundColor = config.LogLevels[logLevel];
                        Console.BackgroundColor = config.LogBackground[logLevel];
                        Console.Write($"{config.LogShort[logLevel]}");

                        Console.ResetColor();
                        Console.WriteLine($": {_name}[{eventId.Id}]\n{formatter(state, exception)}");
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }
    }
}