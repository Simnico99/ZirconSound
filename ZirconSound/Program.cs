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
using System;
using System.IO;
using System.Threading.Tasks;
using ZirconSound.ApplicationCommands.Interactions;
using ZirconSound.Embeds;
using ZirconSound.Logger;
using ZirconSound.Services;

namespace ZirconSound
{
    /// <summary>
    ///     The entry point of the bot.
    /// </summary>
    internal static class Program
    {
        private static IConfiguration _configuration;

        private static async Task Main()
        {
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true);

            _configuration = configBuilder.Build();


            var builder = new HostBuilder()
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
                    services.AddSingleton<IAudioService, LavalinkNode>().AddSingleton(new LavalinkNodeOptions
                    {
                        RestUri = "http://localhost:2333/",
                        WebSocketUri = "ws://localhost:2333/",
                        Password = "youshallnotpass"
                    });

                    services.AddHostedService<DiscordSocketService>();
                    services.AddHostedService<InteractionService>();
                })
                .UseConsoleLifetime()
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.ClearProviders().AddColorConsoleLogger(configuration =>
                    {
                        configuration.LogLevels.Add(
                            LogLevel.Critical, ConsoleColor.Red);
                    });
                    x.SetMinimumLevel(_configuration.GetSection("Logging").GetSection("LogLevel").GetValue<LogLevel>("Default"));
                });

            var host = builder.Build();
            var discordSocket = host.Services.GetRequiredService<ILogger<LavalinkService>>();
            LavalinkService.Start(discordSocket);

            using (host)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(4));
                    await host.RunAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Software existed with the following error:");
                    Console.WriteLine(ex.Message);

                    Console.ReadKey();

                    Environment.Exit(ex.HResult);
                }

                Environment.Exit(0);
            }
        }
    }
}