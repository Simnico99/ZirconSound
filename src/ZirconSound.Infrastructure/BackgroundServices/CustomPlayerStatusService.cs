using Discord;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Player;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZirconSound.Application.Services;

namespace ZirconSound.Infrastructure.BackgroundServices;
public class CustomPlayerStatusService : BackgroundService
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly LavalinkRunnerService _lavalinkRunnerService;
    private readonly ILogger _logger;
    private readonly IAudioService _audioService;
    private readonly ICustomPlayerService _customPlayerService;

    public CustomPlayerStatusService(DiscordSocketClient discordSocketClient, ILogger<CustomPlayerStatusService> logger, IAudioService audioService, ICustomPlayerService customPlayerService, LavalinkRunnerService lavalinkRunnerService)
    {
        _audioService = audioService;
        _customPlayerService = customPlayerService;
        _logger = logger;
        _discordSocketClient = discordSocketClient;
        _lavalinkRunnerService = lavalinkRunnerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _discordSocketClient.WaitForReadyAsync(stoppingToken);

        _lavalinkRunnerService.IsReady.WaitOne();

        await _audioService.InitializeAsync();

        _logger.LogInformation("CustomPlayerStatusService status service is ready!");

        _discordSocketClient.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
    }

    private int NumberOfUserInChannel(SocketGuildChannel socketChannel)
    {
        var voiceUsers = _discordSocketClient.Guilds.FirstOrDefault(
                x => x.Name.Equals(socketChannel.Guild.Name))
            ?.VoiceChannels.FirstOrDefault(
                x => x.Name.Equals(socketChannel.Name))
            ?.Users;

        return voiceUsers!.Count;
    }

    private bool IsBotInChannel(SocketGuildChannel socketChannel)
    {
        var bot = socketChannel.Users.FirstOrDefault(x => x.Id.Equals(_discordSocketClient.CurrentUser.Id));
        return bot != null;
    }

    private void DisconnectBot(IChannel voiceState, LavalinkPlayer? player)
    {
        _logger.LogDebug("Bot is alone initiating disconnect. Id:{VoiceId} / Name:{VoiceName}", voiceState.Id, voiceState.Name);
        _customPlayerService.StartDisconnectBotIsAloneTimer(player, TimeSpan.FromSeconds(30));
    }

    private async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState voiceState1, SocketVoiceState voiceState2)
    {
        var voiceSocket1 = voiceState1.VoiceChannel;
        var voiceSocket2 = voiceState2.VoiceChannel;

        if (!(voiceSocket1 == voiceSocket2))
        {
            if (voiceSocket1 is not null)
            {
                if (IsBotInChannel(voiceSocket1))
                {
                    var player = _audioService.GetPlayer(voiceSocket1.Guild.Id);
                    if (voiceSocket1?.Id == player?.VoiceChannelId)
                    {
                        if (voiceSocket1 is not null && NumberOfUserInChannel(voiceSocket1) > 2)
                        {
                            _logger.LogDebug("Bot is not alone anymore canceling disconnect. Id:{VoiceId} / Name:{VoiceName}", voiceSocket1?.Id, voiceSocket1?.Name);
                            _customPlayerService.CancelAloneDisconnect(player);
                        }
                        else
                        {
                            _ = Task.Run(() => DisconnectBot(voiceSocket1!, player));
                        }
                    }
                }
            }
        }

        if (voiceSocket2 is not null)
        {
            if (IsBotInChannel(voiceSocket2))
            {
                var player = _audioService.GetPlayer(voiceSocket2.Guild.Id);
                if (voiceSocket2?.Id == player?.VoiceChannelId)
                {
                    if (voiceSocket2 is not null && NumberOfUserInChannel(voiceSocket2) >= 2)
                    {
                        _logger.LogDebug("Bot is not alone anymore canceling disconnect. Id:{VoiceId} / Name:{VoiceName}", voiceSocket2.Id, voiceSocket2.Name);
                        _customPlayerService.CancelAloneDisconnect(player);
                    }
                    else
                    {
                        _ = Task.Run(() => DisconnectBot(voiceSocket2!, player));
                    }
                }
            }
        }
    }
}
