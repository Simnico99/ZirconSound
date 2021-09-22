using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using ZirconSound.Services;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Player;
using ZirconSound.DiscordHandlers;
using ZirconSound.Player;

namespace ZirconSound
{
    /// <summary>
    /// The entry point of the bot.
    /// </summary>
    internal class Program
    {
        private static async Task Main()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost((context, config) =>
                { 
                    config.SocketConfig = new DiscordSocketConfig()
                    {
                        LogLevel = LogSeverity.Debug,
                        AlwaysDownloadUsers = false,
                        MessageCacheSize = 200,
                    };

                    config.Token = context.Configuration["Token"];
                })
                .UseCommandService((context, config) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Debug;
                    config.DefaultRunMode = RunMode.Async;
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<EmbedHandler>();
                    services.AddSingleton<PlayerService>();
                    services.AddHostedService<CommandHandler>();
                    /*
                    services.AddHostedService<BotStatusService>();
                    services.AddSingleton<YoutubeModule>();
                    services.AddSingleton<AudioServiceOld>();
                    services.AddSingleton<LavalinkAudioService>();
                    services.AddSingleton<ZirconEmbedBuilder>();
                    */
                    services.AddSingleton<QueuedLavalinkPlayer>();
                    services.AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();
                    services.AddSingleton<IAudioService, LavalinkNode>().AddSingleton(new LavalinkNodeOptions
                    {
                        RestUri = "http://localhost:2333/",
                        WebSocketUri = "ws://localhost:2333/",
                        Password = "youshallnotpass"
                    });

                    services.AddHostedService<DiscordSocketService>();

                })
                .UseConsoleLifetime();


            var host = builder.Build();
            var discordSocket = host.Services.GetRequiredService<ILogger<LavalinkJar>>();
            LavalinkJar.Start(discordSocket);
            using (host)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    host.Run();
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

