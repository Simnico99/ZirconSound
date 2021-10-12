namespace ZirconSound.Hosting.Helpers;

internal class ServiceRegistrationHost : IHostedService
{
    private readonly LogAdapter<InteractionsService> _adapter;
    private readonly InteractionsService _commandService;
    private readonly ILogger<ServiceRegistrationHost> _logger;

    public ServiceRegistrationHost(InteractionsService commandService, ILogger<ServiceRegistrationHost> logger, LogAdapter<InteractionsService> adapter)
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
