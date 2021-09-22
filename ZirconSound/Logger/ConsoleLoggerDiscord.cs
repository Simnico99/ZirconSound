using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZirconSound.DiscordHandlers;
using ZirconSound.Enum;

namespace ZirconSound.Logger
{
    public class ConsoleLoggerDiscord
    {
        private readonly DiscordSocketClient _client;
        private readonly EmbedHandler _embedHandler;
        public Dictionary<LogLevel, ZirconEmbedType> LogLevels { get; set; } = new()
        {
            [LogLevel.Information] = ZirconEmbedType.Info,
            [LogLevel.Warning] = ZirconEmbedType.Warning,
            [LogLevel.Error] = ZirconEmbedType.Error,
            [LogLevel.Debug] = ZirconEmbedType.Debug
        };

        public ConsoleLoggerDiscord(DiscordSocketClient client)
        {
            _client = client;
            _embedHandler = new EmbedHandler(client);
        }


        public async Task LogToChannel(LogLevel logLevel, Exception exception, EventId eventId, string text, string name)
        {
            if (_client.ConnectionState == Discord.ConnectionState.Connected)
            {
                if (!name.Contains("Discord.WebSocket.DiscordSocketClient") && !text.Contains("POST"))
                {
                    if (Program.Configuration.GetSection("Logging").GetSection("LogLevel").GetValue<LogLevel>("Discord") <= logLevel)
                    {
                        var logChannel = _client.GetChannel(890242665256460339) as IMessageChannel;
                        var embed = _embedHandler.Create();
                        embed.WithTitle(logLevel.ToString());
                        embed.AddField("Process name", name);
                        embed.AddField("Output", text);

                        await logChannel.SendMessageAsync(embed: embed.BuildSync(LogLevels[logLevel]));
                    }
                }
            }
        }
    }
}
