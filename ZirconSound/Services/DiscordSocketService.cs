using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZirconSound.Player;

namespace ZirconSound.Services
{
    public class DiscordSocketService : DiscordClientService
    {
        public new DiscordSocketClient Client { get; }
        private new ILogger Logger { get; set; }
        private IAudioService AudioService { get; set; }
        private readonly PlayerService _playerService;

        public DiscordSocketService(DiscordSocketClient client, ILogger<DiscordSocketService> logger, IAudioService audioService, PlayerService playerService) : base(client, logger)
        {
            Client = client;
            Logger = logger;
            AudioService = audioService;
            _playerService = playerService;
        }

        private static string GetPlural<T>(IEnumerable<T> enumberable) 
        {
            var plural = "";
            if (enumberable.Count() > 1)
            {
                plural = "s";
            }
            return plural;
        }

        private async Task StatusLoop()
        {
            var timeSpan = TimeSpan.FromSeconds(20);
            while (true)
            {
                await Client.SetActivityAsync(new Game("!help for commands"));
                Logger.LogDebug("Setting activity");

                await Task.Delay(timeSpan);

                var guilds = Client.Guilds;
                await Client.SetActivityAsync(new Game($"in {guilds.Count} server{GetPlural(guilds)}!"));
                Logger.LogDebug("Setting activity");

                await Task.Delay(timeSpan);

                var player = AudioService.GetPlayers<QueuedLavalinkPlayer>();
                await Client.SetActivityAsync(new Game($" {player.Count} track{GetPlural(player)}!"));
                Logger.LogDebug("Setting activity");

                await Task.Delay(timeSpan);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait for the client to be ready before setting the status

            await Client.WaitForReadyAsync(stoppingToken);
            Logger.LogInformation("Client is ready!");

            await AudioService.InitializeAsync();
            Logger.LogInformation("Audio Service is ready!");

            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;

            await Task.Run(() => StatusLoop(), stoppingToken);
        }

        private int NumberOfUserInChannel(SocketVoiceChannel socketChannel)
        {
            var voiceUsers = Client.Guilds.FirstOrDefault(
            x => x.Name.Equals(socketChannel.Guild.Name)).VoiceChannels.FirstOrDefault(
            x => x.Name.Equals(socketChannel.Name)).Users;

            return voiceUsers.Count;
        }

        private bool IsBotInChannel(SocketVoiceChannel socketChannel)
        {
            var bot = socketChannel.Users.FirstOrDefault(x => x.Id.Equals(Client.CurrentUser.Id));
            if (bot != null)
            {
                return true;
            }
            return false;
        }

        private async Task DisconnectBot(SocketVoiceChannel voiceState, LavalinkPlayer player) 
        {
            _ = Task.Run(async () =>
            {
                Logger.LogDebug($"Bot is alone initiating disconnect. Id:{voiceState.Id} / Name:{voiceState.Name}");
                await _playerService.BotIsAloneAsync(player, TimeSpan.FromSeconds(30));
                return Task.CompletedTask;
            });
            await Task.Delay(0);
        }

        private async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState voiceState1, SocketVoiceState voiceState2)
        {
            var voiceSocket1 = voiceState1.VoiceChannel;
            var voiceSocket2 = voiceState2.VoiceChannel;

            if (voiceSocket1 != null)
            {
                if (IsBotInChannel(voiceSocket1))
                {
                    var player = AudioService.GetPlayer(voiceSocket1.Guild.Id);
                    if (NumberOfUserInChannel(voiceSocket1) >= 2)
                    {
                        Logger.LogDebug($"Bot is not alone anymore canceling disconnect. Id:{voiceSocket1.Id} / Name:{voiceSocket1.Name}");
                        await _playerService.CancelAloneDisconnectAsync(player);
                    }
                    else
                    {
                        await DisconnectBot(voiceSocket1, player);
                    }
                }
            }
            if (voiceSocket2 != null)
            {
                if (IsBotInChannel(voiceSocket2))
                {
                    var player = AudioService.GetPlayer(voiceSocket2.Guild.Id);
                    if (NumberOfUserInChannel(voiceSocket2) >= 2)
                    {
                        Logger.LogDebug($"Bot is not alone anymore canceling disconnect. Id:{voiceSocket2.Id} / Name:{voiceSocket2.Name}");
                        await _playerService.CancelAloneDisconnectAsync(player);
                    }
                    else
                    {
                        await DisconnectBot(voiceSocket2, player);
                    }
                }
            }
        }
    }
}
