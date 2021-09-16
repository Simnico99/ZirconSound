using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Net.Rest;

namespace ZirconSound.Services
{
    public class BotStatusService : DiscordClientService
    {
        private new DiscordSocketClient Client { get; }
        private new ILogger Logger { get; set; }
        private AudioService AudioService { get; set; }

        public BotStatusService(DiscordSocketClient client, ILogger<DiscordClientService> logger, AudioService audioService) : base(client, logger)
        {
            Client = client;
            Logger = logger;
            AudioService = audioService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait for the client to be ready before setting the status
            await Client.WaitForReadyAsync(stoppingToken);
            Logger.LogInformation("Client is ready!");

            await Client.SetActivityAsync(new Game("!help for commands"));

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
                if (IsBotInChannel(voiceSocket1) ) 
                {
                    var player = AudioService.LavaNode.GetPlayer(voiceSocket1.Guild);
                    if (NumberOfUserInChannel(voiceSocket1) >= 2)
                    {
                    }
                    else 
                    {
                        Logger.LogInformation("Disconnecting!!");
                        await AudioService.InitiateDisconnectAsync(player, TimeSpan.FromSeconds(0));
                    }
                }
            }
            if (voiceSocket2 != null)
            {
                if (IsBotInChannel(voiceSocket2))
                {
                    var player = AudioService.LavaNode.GetPlayer(voiceSocket2.Guild);
                    if (NumberOfUserInChannel(voiceSocket2) >= 2)
                    {
                    }
                    else
                    {
                        Logger.LogInformation("Disconnecting!!");
                        await AudioService.InitiateDisconnectAsync(player, TimeSpan.FromSeconds(0));
                    }
                }
            }
        }
    }
}
