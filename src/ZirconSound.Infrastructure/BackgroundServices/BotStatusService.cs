using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Lavalink4NET;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ZirconSound.Infrastructure.BackgroundServices;
public class BotStatusService : DiscordClientService
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly ILogger _logger;
    private readonly IAudioService _audioService;
    private readonly IConfiguration _configuration;

    public BotStatusService(DiscordSocketClient discordSocketClient, ILogger<BotStatusService> logger, IAudioService audioService, IConfiguration configuration)
       : base(discordSocketClient, logger)
    {
        _logger = logger;
        _audioService = audioService;
        _discordSocketClient = discordSocketClient;
        _configuration = configuration;
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

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BotSatusService service is ready!");
#if DEBUG
        var timeSpan = TimeSpan.FromSeconds(5);
#else
        var timeSpan = TimeSpan.FromSeconds(60);
#endif
        while (true)
        {
            await _discordSocketClient.SetActivityAsync(new Game("/help for commands"));
            _logger.LogDebug("Setting activity");

            await Task.Delay(timeSpan);

            await _discordSocketClient.SetActivityAsync(new Game($"in shard #{_configuration["Shards:ShardName"]!.Split("-").Last()}"));
            _logger.LogDebug("Setting activity");

            await Task.Delay(timeSpan);
            //var guilds = _discordSocketClient.Guilds;
            //await _discordSocketClient.SetActivityAsync(new Game($"in {guilds.Count} server{GetPlural(guilds)}!"));
            //_logger.LogDebug("Setting activity");

            //var player = _audioService.Players.GetPlayers<QueuedLavalinkPlayer>();
            //await _discordSocketClient.SetActivityAsync(new Game($" {player.Count()} track{GetPlural(player)}!"));
            //_logger.LogDebug("Setting activity");

            await Task.Delay(timeSpan);
        }
    }
}
