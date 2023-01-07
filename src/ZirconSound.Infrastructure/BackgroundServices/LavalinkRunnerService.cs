using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ZirconSound.Core.Exceptions;
using ZirconSound.Core.Helpers;

namespace ZirconSound.Infrastructure.BackgroundServices;
public sealed class LavalinkRunnerService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly Process _process;
    public EventWaitHandle IsReady { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);

    public LavalinkRunnerService(ILogger<LavalinkRunnerService> logger)
    {
        _logger = logger;

        var path = Directory.GetCurrentDirectory();

        _process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = $@"-jar {path}\Lavalink\Lavalink.jar ",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //* Set your output and error (asynchronous) handlers
        _process.OutputDataReceived += OutputHandler;
        _process.ErrorDataReceived += OutputHandler;
        //* Start process and handlers
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _process?.Kill(true);
        _logger.LogInformation("Killed all instances of lavalink.");
    }

    private void OutputHandler(object sender, DataReceivedEventArgs e)
    {
        var message = e.Data;
        if (!string.IsNullOrEmpty(message))
        {
            var lastIndexOf = message.LastIndexOf(']');
            var cleanMessage = message;

            if (lastIndexOf > 0)
            {
                cleanMessage = message[(lastIndexOf + 1)..];
            }

            if (message.Contains("INFO"))
            {
                _logger.LogInformation("Lavalink Jar: {Message}", cleanMessage);
            }
            else if (message.Contains("WARN"))
            {
                _logger.LogWarning("Lavalink Jar: {Message}", cleanMessage);
            }
            else if (message.Contains("ERROR"))
            {
                _logger.LogError("Lavalink Jar: {Message}", cleanMessage);
            }

            if (message.Contains("https://github.com/Frederikam/Lavalink/issues/295"))
            {
                IsReady.Set();
            }

            if (message.Contains("lavalink.server.Launcher                 : Application failed"))
            {
                var exception = new LavalinkAlreadyRunningException("Lavalink is already running please close the other instance then try again.");
                _logger.LogCritical(exception,"Cannot run:");

                Console.ReadLine();
                throw exception;
            }
        }
    }
}
