using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace ZirconSound.Infrastructure.BackgroundServices;
public class BotStatusService : DiscordShardedClientService
{
    private readonly DiscordShardedClient _discordShardedClient;
    private readonly ILogger _logger;

    public BotStatusService(DiscordShardedClient discordShardedClient, ILogger<BotStatusService> logger)
       : base(discordShardedClient, logger)
    {
        _logger = logger;
        _discordShardedClient = discordShardedClient;
    }

    private static string GetPlural(int number)
    {
        var plural = "";
        if (number > 1)
        {
            plural = "s";
        }

        return plural;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordShardedClient.ShardReady += async (discordSocketClient) =>
        {
            _logger.LogInformation("BotSatusService service is ready!");
#if DEBUG
            var timeSpan = TimeSpan.FromSeconds(5);
#else
        var timeSpan = TimeSpan.FromSeconds(30);
#endif
            while (!stoppingToken.IsCancellationRequested)
            {
                await discordSocketClient.SetActivityAsync(new Game("/help for commands"));
                _logger.LogDebug("Setting activity");
                await Task.Delay(timeSpan);

                await discordSocketClient.SetActivityAsync(new Game($"in shard #{discordSocketClient.ShardId}"));
                _logger.LogDebug("Setting activity");
                await Task.Delay(timeSpan);

                //var player = _audioService.Players.GetPlayers<QueuedLavalinkPlayer>();
                //await _discordShardedClient.SetActivityAsync(new Game($" {player.Count()} track{GetPlural(player)}!"));
                //_logger.LogDebug("Setting activity");
            }
        };
        return Task.CompletedTask;
    }
}
