using Discord;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZirconSound.Application.Interfaces;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;

namespace ZirconSound.Infrastructure.BackgroundServices;
public class CustomPlayerService : BackgroundService
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly ILavalinkRunnerService _lavalinkRunnerService;
    private readonly ILogger _logger;
    private readonly IAudioService _audioService;

    public CustomPlayerService(DiscordSocketClient discordSocketClient, ILogger<CustomPlayerService> logger, IAudioService audioService, ILavalinkRunnerService lavalinkRunnerService)
    {
        _audioService = audioService;
        _logger = logger;
        _discordSocketClient = discordSocketClient;
        _lavalinkRunnerService = lavalinkRunnerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _discordSocketClient.WaitForReadyAsync(stoppingToken);

        _lavalinkRunnerService.IsReady.WaitOne();

        await _audioService.InitializeAsync();
        _audioService.TrackEnd += AudioService_TrackEnd;
        _discordSocketClient.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;

        _logger.LogInformation("CustomPlayerStatusService status service is ready!");
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
        LavalinkPlayerHelper.StartDisconnectBotIsAloneTimer(player, TimeSpan.FromSeconds(30));
    }

    private Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState voiceState1, SocketVoiceState voiceState2)
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
                            LavalinkPlayerHelper.CancelAloneDisconnect(player);
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
                        LavalinkPlayerHelper.CancelAloneDisconnect(player);
                    }
                    else
                    {
                        _ = Task.Run(() => DisconnectBot(voiceSocket2!, player));
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
    private async Task OnError(GenericQueuedLavalinkPlayer player, TrackEndReason trackEndReason)
    {
        _logger.LogWarning("Track {TackTitle} threw an exception.", player.CurrentTrack?.Title);

        if (player.PlayerGotError || player.CurrentTrack is null)
        {
            player.PlayerGotError = false;
            string? errorMessage;

            if (player.CurrentTrack is not null && trackEndReason is not TrackEndReason.Replaced)
            {
                errorMessage = $"An error occured with a song ({player.CurrentTrack.Title}):\nThe song will be skipped and removed from queue.";
                player.Queue.Remove(player.CurrentTrack);
            }
            else
            {
                errorMessage = $"An error occured with a song (Unable to get the song title):\nThe song will be skipped.";
            }

            if (player.Context is not null)
            {
                var embed = EmbedHelpers.CreateGenericEmbedBuilder(player.Context);
                embed.AddField("Error", errorMessage);
                await player.Context.Interaction.FollowupAsync(embed: embed.Build(GenericEmbedType.Error));
            }

            return;
        }

        player.PlayerGotError = true;
        await player.PlayAsync(player.CurrentTrack);
    }

    private async Task AudioService_TrackEnd(object sender, TrackEndEventArgs eventArgs)
    {
        _logger.LogDebug("Stop reason: {Reason}", eventArgs.Reason);

        if (eventArgs.Player is not GenericQueuedLavalinkPlayer player)
        {
            return;
        }

        if (eventArgs.Reason is not TrackEndReason.LoadFailed and not TrackEndReason.Replaced)
        {

            if (player.CurrentLoopingTrack is not null)
            {
                await player.PlayAsync(player.CurrentLoopingTrack);
                return;
            }

            await DisconnectAndStopIfNoTracksRemainingAsync(player, false, eventArgs.Reason);
        }

        if (eventArgs.Reason is TrackEndReason.LoadFailed or TrackEndReason.Replaced && !player.SkippedOnPurpose)
        {
            await DisconnectAndStopIfNoTracksRemainingAsync(player, true, eventArgs.Reason);
        }
    }

    private async Task DisconnectAndStopIfNoTracksRemainingAsync(GenericQueuedLavalinkPlayer player, bool errorCheck, TrackEndReason trackEndReason)
    {
        if (errorCheck)
        {
            await OnError(player, trackEndReason);
        }

        if (player.PlayerGotError)
        {
            return;
        }

        if (player.CurrentLoopingPlaylist is not null && player.CurrentLoopingPlaylist.Count > 0)
        {
            if (player.CurrentTrack == player.CurrentLoopingPlaylist.Last())
            {
                player.Queue.Clear();
                await player.PlayAsync(player.CurrentLoopingPlaylist.First());
                var playlist = player.CurrentLoopingPlaylist.Skip(1);

                foreach (var track in playlist)
                {
                    player.Queue.Add(track);
                }

                return;
            }
        }

        if (player.Queue is null || player.Queue.Count <= 0 || player.Queue.Last() == player.CurrentTrack || player.State != PlayerState.Playing || trackEndReason is TrackEndReason.Finished)
        {
            await player.StopAsync();
            LavalinkPlayerHelper.StartIdleDisconnectTimer(player, TimeSpan.FromSeconds(30));
            return;
        }

        await player.SkipAsync();
    }
}
