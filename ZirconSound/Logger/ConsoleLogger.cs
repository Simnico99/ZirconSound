using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ZirconSound.Logger
{
    public class ConsoleLogger : ILogger
    {
        private readonly string _name;
        private readonly Func<ConsoleLoggerConfiguration> _getCurrentConfig;
        private readonly ConsoleLoggerDiscord _loggerDiscord;

        public ConsoleLogger(
            string name, ConsoleLoggerDiscord loggerDiscord,
            Func<ConsoleLoggerConfiguration> getCurrentConfig) =>
            (_name, _loggerDiscord, _getCurrentConfig) = (name, loggerDiscord, getCurrentConfig);

        public IDisposable BeginScope<TState>(TState state) => default;

        public bool IsEnabled(LogLevel logLevel) =>
            _getCurrentConfig().LogLevels.ContainsKey(logLevel);

        public async void Log<TState>(
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

            ConsoleLoggerConfiguration config = _getCurrentConfig();
            if (config.EventId == 0 || config.EventId == eventId.Id)
            {
                ConsoleColor originalColor = ConsoleColor.White;
                ConsoleColor originalBckColor = ConsoleColor.Black;


                Console.ForegroundColor = config.LogLevels[logLevel];
                Console.BackgroundColor = config.LogBackground[logLevel];
                Console.Write($"{config.LogShort[logLevel]}");

                Console.ForegroundColor = originalColor;
                Console.BackgroundColor = originalBckColor;
                Console.WriteLine($": {_name}[{eventId.Id}]");
                Console.WriteLine($"{formatter(state, exception)}");

                try
                {
                    await Task.Run(() => _loggerDiscord.LogToChannel(logLevel, exception, eventId, formatter(state, exception), _name));
                }
                catch (Discord.Net.HttpException)
                { 
                    //--Ignore
                }
                
            }
        }
    }

}
