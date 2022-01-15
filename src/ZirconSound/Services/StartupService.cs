using Serilog;
using ZirconSound.Handlers;
using ZirconSound.Helpers;
using ZirconSound.Hosting.Extensions;

namespace ZirconSound.Services;

internal sealed class StartupService : IDisposable
{
    private readonly IConfiguration _configuration;
    private bool _disposedValue;
    private IHost Host { get; }

    public StartupService()
    {
        _configuration = BuildConfig(new ConfigurationBuilder());
        SetLog();
        Host = CreateHost();
    }

    private void SetLog()
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom
            .Configuration(_configuration)
            .CreateLogger();

        Log.Information("Starting {SoftwareName} up!", "ZirconSound");
        Log.Information("Environment: {Environment}", Environment.GetEnvironmentVariable("DOTNET_") ?? "Production");
    }

    public async Task Start()
    {
        LavalinkService.Start(Host.Services.GetRequiredService<ILogger<LavalinkService>>());
        LavalinkService.IsReady.WaitOne();
        await Host.RunAsync();
        LavalinkService.CloseProcess();
    }

    public static IConfiguration BuildConfig(IConfigurationBuilder builder) => builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_") ?? "Production" }.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

    private static IHost CreateHost() => new HostBuilder()
        .ConfigureAppConfiguration(x =>
        {
            x.AddConfiguration(BuildConfig(new ConfigurationBuilder()));
        })
        .ConfigureDiscordHost((context, config) =>
        {
            config.SocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = false,
                GatewayIntents = 
                GatewayIntents.Guilds |
                GatewayIntents.GuildBans |
                GatewayIntents.GuildEmojis |
                GatewayIntents.GuildIntegrations |
                GatewayIntents.GuildWebhooks |
                GatewayIntents.GuildVoiceStates |
                GatewayIntents.GuildMessages |
                GatewayIntents.GuildMessageReactions |
                GatewayIntents.GuildMessageTyping |
                GatewayIntents.DirectMessages |
                GatewayIntents.DirectMessageReactions |
                GatewayIntents.DirectMessageTyping,
                MessageCacheSize = 0
            };
            config.Token = context.Configuration["Token"];
            config.LogFormat = (message, exception) => $"{message.Source}: {message.Message}";
        })
        .UseCommandService((context, config) =>
        {
            config.LogLevel = LogSeverity.Info;
            config.DefaultRunMode = RunMode.Async;
        })
        .UseInteractionService((context, config) =>
        {
            config.LogLevel = LogSeverity.Info;
            config.DefaultRunMode = Discord.Interactions.RunMode.Async;
            config.UseCompiledLambda = true;
        })
        .ConfigureServices((_, services) =>
        {
            //Without interface
            services.AddSingleton<ZirconPlayer>();
            services.AddSingleton<LockHelper>();

            //With interface
            services.AddSingleton<IPlayerService, PlayerService>();
            services.AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();

            //Hosted Services
            services.AddHostedService<InteractionHandler>();
            services.AddHostedService<DiscordSocketService>();
        })
        .UseLavalink()
        .UseConsoleLifetime()
        .UseSerilog()
        .Build();

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Host?.Services.GetService<LavalinkNode>()?.Dispose();
            }
            Host?.Dispose();

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
