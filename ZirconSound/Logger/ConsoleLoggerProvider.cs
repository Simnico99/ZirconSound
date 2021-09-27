using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ZirconSound.Logger
{
    public sealed class ConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ConsoleLogger> _loggers = new();
        private readonly IDisposable _onChangeToken;
        private ConsoleLoggerConfiguration _currentConfig;


        public ConsoleLoggerProvider(IOptionsMonitor<ConsoleLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new ConsoleLogger(name, GetCurrentConfig));
        }

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }

        private ConsoleLoggerConfiguration GetCurrentConfig()
        {
            return _currentConfig;
        }
    }
}