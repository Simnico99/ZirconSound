using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET.Clients;
using Lavalink4NET.Cluster.Extensions;
using Lavalink4NET.Cluster.Nodes;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.InactivityTracking.Extensions;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioIsNotPlaying;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioPlaying;
using ZirconSound.Application.Handlers;
using ZirconSound.Infrastructure.BackgroundServices;

namespace ZirconSound.Console;
public static class DependencyInjection
{
    public static IHostBuilder AddServices(this IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddMediator();
            services.AddHostedService<BotStatusService>();

            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(AudioAutoJoinBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(AudioPlayingBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(AudioIsNotPlayingBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(AudioIsInVoiceChannelBehavior<,>));
            services.AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();
            services.AddHostedService<InteractionHandler>();
        });

        return builder;
    }

    public static IHostBuilder AddDiscordServices(this IHostBuilder builder, IConfiguration configuration)
    {
        builder.ConfigureServices(services =>
        {

            services.AddDiscordShardedHost((discordConfig, services) =>
            {
                discordConfig.SocketConfig = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    AlwaysDownloadUsers = false,
                    GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildEmojis |
                    GatewayIntents.GuildIntegrations |
                    GatewayIntents.GuildVoiceStates |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.GuildMessageReactions |
                    GatewayIntents.DirectMessages |
                    GatewayIntents.DirectMessageReactions |
                    GatewayIntents.DirectMessageTyping,
                    MessageCacheSize = 0,
                    TotalShards = configuration.GetRequiredSection("Shards").GetValue<int>("TotalShards"),
                };

                discordConfig.ShardIds = [Convert.ToInt32(configuration["Shards:ShardName"]!.Split("-").Last())];
                discordConfig.Token = configuration["Token"]!;
                discordConfig.LogFormat = (message, exception) => $"{message.Source}: {message.Message}";
            });

            services.AddCommandService((config, services) =>
            {
                config.LogLevel = LogSeverity.Info;
                config.DefaultRunMode = RunMode.Async;
            });

            services.AddInteractionService((config, services) =>
            {
                config.LogLevel = LogSeverity.Info;
                config.DefaultRunMode = Discord.Interactions.RunMode.Async;
                config.UseCompiledLambda = true;
            });

        });

        return builder;
    }

    public static IHostBuilder UseLavalink(this IHostBuilder builder, IConfiguration configuration)
    {
        builder.ConfigureServices(services =>
        {
            services.AddLavalinkCluster<DiscordClientWrapper>();

            services.ConfigureLavalinkCluster(x => x.Nodes =
            [
                    new LavalinkClusterNodeOptions
                    {
                        BaseAddress = new Uri(configuration["Lavalink:Node1"] ?? string.Empty),
                        Passphrase = configuration["Lavalink:NodesPassword"] ?? string.Empty,
                    },
#if !DEBUG
                    new LavalinkClusterNodeOptions
                    {
                        BaseAddress = new Uri(configuration["Lavalink:Node2"] ?? string.Empty),
                        Passphrase = configuration["Lavalink:NodesPassword"] ?? string.Empty,
                    }
#endif
            ]);

            services.AddInactivityTracking();
            services.ConfigureInactivityTracking(config => config.DefaultTimeout = TimeSpan.FromMinutes(1));
        });
        return builder;
    }

    public static IHostBuilder ConfigureLoggers(this IHostBuilder builder, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
        .ReadFrom
        .Configuration(configuration)
        .CreateLogger();

        Log.Information("Starting {SoftwareName} up!", AppDomain.CurrentDomain.FriendlyName);
        Log.Information("Environment: {Environment}", Environment.GetEnvironmentVariable("DOTNET_") ?? "Production");
        Log.Information("Version: {CurrentVersion}", Assembly.GetExecutingAssembly().GetName().Version);

        return builder;
    }

    public static IConfigurationBuilder RegisterConfigurations(this IConfigurationBuilder configuration)
    {
        configuration.SetBasePath(Directory.GetCurrentDirectory());
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configuration.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_") ?? "Production"}.json", optional: true);
        configuration.AddEnvironmentVariables();
        configuration.Build();

        return configuration;
    }
}
