using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using ZirconSound.ApplicationCommands.SlashCommands;

namespace ZirconSound.ApplicationCommands.Hosting
{
    internal class SlashCommandServiceRegistrationHost : IHostedService
    {
        private readonly LogAdapter<SlashCommandService> _adapter;
        private readonly SlashCommandService _commandService;
        private readonly ILogger<SlashCommandServiceRegistrationHost> _logger;

        public SlashCommandServiceRegistrationHost(SlashCommandService commandService, ILogger<SlashCommandServiceRegistrationHost> logger, LogAdapter<SlashCommandService> adapter)
        {
            _commandService = commandService;
            _logger = logger;
            _adapter = adapter;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _commandService.Log += _adapter.Log;
            _logger.LogDebug("Registered logger for CommandService");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}