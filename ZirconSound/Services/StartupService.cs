using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Player;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ZirconSound.ApplicationCommands.Interactions;
using ZirconSound.Embeds;
using ZirconSound.Lavalink4Net;

namespace ZirconSound.Services
{
    internal sealed class StartupService : IDisposable
    {
        private readonly IConfiguration _configuration;
        private bool _disposedValue;
        private IHost Host { get; }

        public StartupService()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            SetLog();
            Host = CreateHost();
        }

        private void SetLog()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(_configuration)
                .CreateLogger();
        }

        public async Task Start()
        {
            LavalinkService.Start(Host.Services.GetRequiredService<ILogger<LavalinkService>>());
            await Task.Delay(TimeSpan.FromSeconds(4));
            await Host.RunAsync();
        }

        private static IHost CreateHost() => new HostBuilder()
            .ConfigureAppConfiguration(x =>
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();

                x.AddConfiguration(configuration);
            })
            .ConfigureDiscordHost((context, config) =>
            {
                config.SocketConfig = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    AlwaysDownloadUsers = false,
                    MessageCacheSize = 200
                };
                config.Token = context.Configuration["Token"];
            })
            .UseInteractionService((_, config) =>
            {
                config.LogLevel = LogSeverity.Info;
                config.DefaultRunMode = RunMode.Async;
            })
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<EmbedHandler>();
                services.AddSingleton<PlayerService>();
                services.AddSingleton<QueuedLavalinkPlayer>();
                services.AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();
                services.AddSingleton<IAudioService, HostedLavalinkNode>().AddSingleton(new LavalinkNodeOptions
                {
                    RestUri = "http://localhost:2333/",
                    WebSocketUri = "ws://localhost:2333/",
                    Password = "youshallnotpass"
                });

                services.AddHostedService<DiscordSocketService>();
                services.AddHostedService<InteractionService>();
            })
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
}
