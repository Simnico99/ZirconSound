using Discord.WebSocket;
using Lavalink4NET;
using Microsoft.Extensions.Hosting;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;

namespace ZirconSound.Infrastructure.BackgroundServices;
public sealed class BotIsAloneOrIdleService : BackgroundService
{
    private readonly IAudioService _audioService;
    private readonly DiscordSocketClient _discordSocketClient;

    public BotIsAloneOrIdleService(IAudioService audioService, DiscordSocketClient discordSocketClient)
    {
        _audioService = audioService;
        _discordSocketClient = discordSocketClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await AloneCheck();
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async ValueTask AloneCheck()
    {
        foreach (var player in _audioService.GetPlayers<GenericQueuedLavalinkPlayer>())
        {
            if (player.VoiceChannelId is not ulong channel)
            {
                continue;
            }


            if (await _discordSocketClient.GetChannelAsync(channel) is SocketVoiceChannel voiceChannel)
            {
                IsAlone(voiceChannel, player);
                IsPlaying(player);
                continue;
            }

            LavalinkPlayerHelper.CancelAloneDisconnect(player);
        }
    }

    private static void IsPlaying(GenericQueuedLavalinkPlayer player)
    {
        if (player.State == Lavalink4NET.Player.PlayerState.Playing)
        {
            if (player.PlayerIsIdle)
            {
                LavalinkPlayerHelper.CancelIdleDisconnect(player);
            }
            return;
        }

        if (!player.PlayerIsIdle)
        {
            LavalinkPlayerHelper.StartIdleDisconnectTimer(player, TimeSpan.FromMinutes(1));
        }
    }

    private static void IsAlone(SocketVoiceChannel voiceSocket, GenericQueuedLavalinkPlayer player)
    {
        if (voiceSocket.ConnectedUsers.Count <= 1 && !player.PlayerIsAlone)
        {
            LavalinkPlayerHelper.StartDisconnectBotIsAloneTimer(player, TimeSpan.FromMinutes(1));
        }
    }
}
