using Discord;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Player;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ZirconSound.Infrastructure.BackgroundServices;
public class BotStatusService : BackgroundService
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly ILogger _logger;
    private readonly IAudioService _audioService;

    public BotStatusService(DiscordSocketClient discordSocketClient, ILogger<CustomPlayerService> logger, IAudioService audioService)
    {
        _logger = logger;
        _audioService = audioService;
        _discordSocketClient = discordSocketClient;
    }

    private static string GetPlural<T>(IEnumerable<T> enumerable)
    {
        var plural = "";
        if (enumerable.Count() > 1)
        {
            plural = "s";
        }

        return plural;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BotSatusService service is ready!");
        var timeSpan = TimeSpan.FromSeconds(60);
        while (!stoppingToken.IsCancellationRequested)
        {
            await _discordSocketClient.SetActivityAsync(new Game("/help for commands"));
            _logger.LogDebug("Setting activity");

            await Task.Delay(timeSpan, stoppingToken);

            var guilds = _discordSocketClient.Guilds;
            await _discordSocketClient.SetActivityAsync(new Game($"in {guilds.Count} server{GetPlural(guilds)}!"));
            _logger.LogDebug("Setting activity");

            await Task.Delay(timeSpan, stoppingToken);

            var player = _audioService.GetPlayers<QueuedLavalinkPlayer>();
            await _discordSocketClient.SetActivityAsync(new Game($" {player.Count} track{GetPlural(player)}!"));
            _logger.LogDebug("Setting activity");

            await Task.Delay(timeSpan, stoppingToken);
        }
    }
}
