using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace ZirconSound.Logger
{
    public sealed class ConsoleLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable _onChangeToken;
        private ConsoleLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, ConsoleLogger> _loggers = new();
        private readonly ConsoleLoggerDiscord _loggerDiscord;

        public ConsoleLoggerProvider(IOptionsMonitor<ConsoleLoggerConfiguration> config, ConsoleLoggerDiscord loggerDiscord)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
            _loggerDiscord = loggerDiscord;
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new ConsoleLogger(name, _loggerDiscord, GetCurrentConfig));

        private ConsoleLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }

}
