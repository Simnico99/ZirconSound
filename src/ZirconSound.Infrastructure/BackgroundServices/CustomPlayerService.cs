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

    private bool IsBotInChannel(SocketGuildChannel socketChannel)
    {
        var bot = socketChannel.Users.FirstOrDefault(x => x.Id.Equals(_discordSocketClient.CurrentUser.Id));
        return bot != null;
    }

    private Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState voiceState1, SocketVoiceState voiceState2)
    {
        var voiceSocket1 = voiceState1.VoiceChannel;
        var voiceSocket2 = voiceState2.VoiceChannel;

        if (!(voiceSocket1 == voiceSocket2) && voiceSocket1 is not null)
        {
            IsNotAloneInThisChannel(voiceSocket1);
        }

        if (voiceSocket2 is not null)
        {
            IsNotAloneInThisChannel(voiceSocket2);
        }

        return Task.CompletedTask;
    }

    private void IsNotAloneInThisChannel(SocketVoiceChannel voiceSocket)
    {
        if (!IsBotInChannel(voiceSocket))
        {
            return;
        }

        if (voiceSocket is not null && voiceSocket.ConnectedUsers.Count >= 2)
        {
            _logger.LogDebug("Bot is not alone anymore canceling disconnect. Id:{VoiceId} / Name:{VoiceName}", voiceSocket.Id, voiceSocket.Name);
            LavalinkPlayerHelper.CancelAloneDisconnect(_audioService.GetPlayer(voiceSocket.Guild.Id));
        }
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
                errorMessage = $"An error occurred with a song ({player.CurrentTrack.Title}):\nThe song will be skipped and removed from queue.";
                player.Queue.Remove(player.CurrentTrack);
                await player.SkipAsync();
            }
            else
            {
                errorMessage = $"An error occurred with a song (Unable to get the song title):\nThe song will be skipped.";
                await player.SkipAsync();
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

            await ErrorCheckAndHandling(player, false, eventArgs.Reason);
        }

        if (eventArgs.Reason is TrackEndReason.LoadFailed or TrackEndReason.Replaced && !player.SkippedOnPurpose)
        {
            await ErrorCheckAndHandling(player, true, eventArgs.Reason);
        }
    }

    private async Task ErrorCheckAndHandling(GenericQueuedLavalinkPlayer player, bool errorCheck, TrackEndReason trackEndReason)
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

        await OnError(player, trackEndReason);
        await player.SkipAsync();
    }
}
