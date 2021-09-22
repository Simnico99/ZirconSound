using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait for the client to be ready before setting the status
            await Client.WaitForReadyAsync(stoppingToken);
            Logger.LogInformation("Client is ready!");

            await Client.SetActivityAsync(new Game("!help for commands"));
            await AudioService.InitializeAsync();
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
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
                        _ = Task.Run(async () =>
                        {
                            Logger.LogDebug($"Bot is alone initiating disconnect. Id:{voiceSocket1.Id} / Name:{voiceSocket1.Name}");
                            await _playerService.BotIsAloneAsync(player, TimeSpan.FromSeconds(30));
                            return Task.CompletedTask;
                        });
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
                        _ = Task.Run(async () =>
                        {
                            Logger.LogDebug($"Bot is alone initiating disconnect. Id:{voiceSocket2.Id} / Name:{voiceSocket2.Name}");
                            await _playerService.BotIsAloneAsync(player, TimeSpan.FromSeconds(30));
                            return Task.CompletedTask;
                        });
                    }
                }
            }
        }

    }
}
