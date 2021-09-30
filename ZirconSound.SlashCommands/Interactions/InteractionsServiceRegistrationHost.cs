using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZirconSound.ApplicationCommands.Helpers;

namespace ZirconSound.ApplicationCommands.Interactions
{
    internal class InteractionsServiceRegistrationHost : IHostedService
    {
        private readonly LogAdapter<InteractionsService> _adapter;
        private readonly InteractionsService _commandService;
        private readonly ILogger<InteractionsServiceRegistrationHost> _logger;

        public InteractionsServiceRegistrationHost(InteractionsService commandService, ILogger<InteractionsServiceRegistrationHost> logger, LogAdapter<InteractionsService> adapter)
        {
            _commandService = commandService;
            _logger = logger;
            _adapter = adapter;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _commandService.Log += _adapter.Log;
            _logger.LogDebug("Registered logger for Interaction Services");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}