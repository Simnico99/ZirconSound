using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Victoria;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ZirconSound.Services;
using ZirconSound.Modules;
using ZirconSound.Common;

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
                    services.AddHostedService<CommandHandler>().AddLavaNode(x =>
                    {
                        x.ReconnectAttempts = 200;
                        x.ReconnectDelay = TimeSpan.FromSeconds(1);
                        x.SelfDeaf = true;
                    });
                    services.AddHostedService<BotStatusService>();
                    services.AddSingleton<AudioService>();
                    services.AddSingleton<ZirconEmbedBuilder>();

                })
                .UseConsoleLifetime();


            var host = builder.Build();
            var lavalink = await StartupService.StartLavalinkAsync();
            using (host)
            {
                using (host)
                {
                        await host.RunAsync().ContinueWith(t => Task.Run(() => lavalink.Kill()), TaskContinuationOptions.None);
  
                }
            }
        }
    }
}

