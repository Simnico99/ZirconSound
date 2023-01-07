using Microsoft.Extensions.Hosting;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Discord.Addons.Hosting;

namespace ZirconSound.Console.Startup;
public static partial class IHostBuilderExtension
{
    public static IHostBuilder ConfigureDiscord(this IHostBuilder host)
    {
        return host.ConfigureDiscordHost((context, config) =>
        {
            config.SocketConfig = new DiscordSocketConfig
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
                MessageCacheSize = 0
            };

            config.Token = context.Configuration["Token"]!;
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
        });
    }
}
