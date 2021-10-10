﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Lavalink4NET;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using ZirconSound.ApplicationCommands.Interactions;
using ZirconSound.ApplicationCommands.MessageComponents;
using ZirconSound.ApplicationCommands.SlashCommands;
using ZirconSound.Embeds;
using ZirconSound.Enum;
using ZirconSound.Extensions;
using ZirconSound.Services;

namespace ZirconSound.Commands
{
    public class AudioCommand : InteractionModule<IInteractionContext>
    {
        private readonly IAudioService _audioService;
        private readonly PlayerService _playerService;
        private ZirconEmbed _errorEmbed;

        public AudioCommand(IAudioService audioService, PlayerService playerService)
        {
            _audioService = audioService;
            _playerService = playerService;
        }

        private bool CheckState(IEnumerable<AudioState> audioStates, IInteractionContext context)
        {
            var player = _audioService.GetPlayer<ZirconPlayer>(context.Guild.Id);
            player?.SetInteraction(Context);
            if (Context.User is IVoiceState voiceState)
            {
                var voiceChannel = voiceState.VoiceChannel;

                var embed = EmbedHandler.Create(Context);

                var enumerable = audioStates as AudioState[] ?? audioStates.ToArray();
                if (enumerable.Contains(AudioState.BotIsInVoiceChannel))
                {
                    if (player == null)
                    {
                        embed.AddField("Warning:", "Is not in a voice channel!");
                        _errorEmbed = embed;
                        return false;
                    }
                }

                if (enumerable.Contains(AudioState.BotIsNotInVoiceChannel))
                {
                    if (player != null)
                    {
                        embed.AddField("Warning:", "Bot is already in a voice channel!");
                        _errorEmbed = embed;
                        return false;
                    }
                }

                if (enumerable.Contains(AudioState.UserIsInVoiceChannel))
                {
                    if (voiceChannel == null)
                    {
                        embed.AddField("Warning:", "You are not in a voice channel!");
                        _errorEmbed = embed;
                        return false;
                    }
                }

                if (enumerable.Contains(AudioState.UserIsNotInVoiceChannel))
                {
                    if (voiceChannel != null)
                    {
                        embed.AddField("Warning:", "You are already in a voice channel!");
                        _errorEmbed = embed;
                        return false;
                    }
                }

                if (enumerable.Contains(AudioState.BotAndUserInSameVoiceChannel))
                {
                    if (voiceChannel?.Id != player?.VoiceChannelId)
                    {
                        embed.AddField("Warning:", "You need to be in the same voice channel!");
                        _errorEmbed = embed;
                        return false;
                    }
                }

                if (enumerable.Contains(AudioState.BotAndUserNotInSameVoiceChannel))
                {
                    if (voiceChannel?.Id != player?.VoiceChannelId)
                    {
                        embed.AddField("Warning:", "You need to be in a different voice channel!");
                        _errorEmbed = embed;
                        return false;
                    }
                }
            }

            return true;
        }

        private async Task JoinAsync()
        {
            var voiceState = Context.User as IVoiceState;
            var voiceChannel = voiceState?.VoiceChannel;
            if (voiceChannel != null)
            {
                await _audioService.JoinAsync<ZirconPlayer>(Context.Guild.Id, voiceChannel.Id, true);
            }
        }

        private static void EmbedSong(ref ZirconEmbed embedBuilder, LavalinkTrack lavalinkTrack)
        {
            var channel = new EmbedFieldBuilder().WithName("Channel").WithValue(lavalinkTrack.Author).WithIsInline(true);
            var duration = new EmbedFieldBuilder().WithName("Duration").WithValue(lavalinkTrack.Duration).WithIsInline(true);


            embedBuilder.WithThumbnailUrl($"https://img.youtube.com/vi/{lavalinkTrack.TrackIdentifier}/0.jpg");
            embedBuilder.AddField(channel);
            embedBuilder.AddField(duration);
        }

        [SlashCommand("play", "Play a track.", "id", ApplicationCommandOptionType.String, "Id/Name/Link/Playlist", true)]
        public async Task PlayAsync([Remainder] string id)
        {
            var embed = EmbedHandler.Create(Context);

            if (CheckState(new List<AudioState> { AudioState.BotIsNotInVoiceChannel }, Context))
            {
                if (CheckState(new List<AudioState> { AudioState.UserIsInVoiceChannel }, Context))
                {
                    await JoinAsync();
                }
                else
                {
                    await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
                    return;
                }
            }

            var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var tracks = await _audioService.GetTracksAsync(id);
                var lavalinkTracks = tracks.ToList();
                if (!lavalinkTracks.Any())
                {
                    var track = await _audioService.GetTrackAsync(id, SearchMode.YouTube, true);
                    lavalinkTracks = new List<LavalinkTrack> { track };
                }

                if (lavalinkTracks.FirstOrDefault() != null)
                {
                    if (player?.CurrentTrack == null)
                    {
                        await _playerService.CancelDisconnectAsync(player);
                        var track = lavalinkTracks.First();
                        embed.AddField("Playing:", $"[{track.Title}]({track.Source})");
                        EmbedSong(ref embed, track);
                        lavalinkTracks = lavalinkTracks.Skip(1).ToList();
                        if (player != null)
                        {
                            await player.PlayAsync(track);
                        }
                    }
                    else
                    {
                        if (lavalinkTracks.Count <= 1)
                        {
                            var track = lavalinkTracks.First();
                            lavalinkTracks = lavalinkTracks.Skip(1).ToList();

                            embed.AddField("Queued:", $"[{track.Title}]({track.Source})");
                            EmbedSong(ref embed, track);

                            var timeLeft = TimeSpan.FromSeconds(0);
                            timeLeft = player.Queue.Tracks.Aggregate(timeLeft, (current, trackQueue) => current + trackQueue.Duration);
                            timeLeft += player.CurrentTrack.Duration - player.Position.Position.StripMilliseconds();

                            var estimatedTime = new EmbedFieldBuilder().WithName("Estimated time until track").WithValue(timeLeft).WithIsInline(true);

                            embed.AddField(estimatedTime);
                            embed.AddField("Position in queue", player.Queue.Tracks.Count + 1);
                            player.Queue.Add(track);
                        }
                    }

                    if (lavalinkTracks.Count > 1)
                    {
                        var estimatedTime = new EmbedFieldBuilder().WithName("Queued:").WithValue($"{lavalinkTracks.Count} tracks!").WithIsInline(true);

                        embed.AddField(estimatedTime);

                        var timeLeft = TimeSpan.FromSeconds(0);
                        foreach (var track in lavalinkTracks)
                        {
                            timeLeft += track.Duration;
                            player?.Queue.Add(track);
                        }

                        embed.AddField("Estimated play time:", $"{timeLeft}");
                    }

                    await Context.ReplyToCommandAsync(embed: embed.BuildSync());
                    return;
                }

                embed.AddField("Warning:", "Unable to find the specified track!");
                await Context.ReplyToCommandAsync(embed: embed.BuildSync(ZirconEmbedType.Warning), ephemeral: false);
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
            }
        }

        [SlashCommand("stop", "Stop the current track.")]
        public async Task StopAsync()
        {
            var embed = EmbedHandler.Create(Context);

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);
                if (player is { State: PlayerState.Playing or PlayerState.Paused })
                {
                    embed.AddField("Stopped", "Stopped current track!");
                    await Context.ReplyToCommandAsync(embed: embed.BuildSync());

                    await player.StopAsync();
                }
                else
                {
                    embed.AddField("Unable to stop:", "No track are actually playing couln't stop the track!");
                    await Context.ReplyToCommandAsync(embed: embed.BuildSync(ZirconEmbedType.Warning));
                }
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
            }
        }

        [SlashCommand("skip", "Skip the current track.")]
        public async Task SkipAsync()
        {
            var embed = EmbedHandler.Create(Context);

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);

                if (player != null && player.Queue.Count > 0)
                {
                    var track = player.Queue.FirstOrDefault();

                    embed.AddField("Skipped now playing:", $"[{track?.Title}]({track?.Source})");
                    EmbedSong(ref embed, track);

                    await player.SkipAsync();

                    if (player.Queue.Count > 1)
                    {
                        var queueLength = new EmbedFieldBuilder().WithName("Queue count").WithValue($"{player.Queue.Count} tracks").WithIsInline(true);
                        embed.AddField(queueLength);
                    }

                    await Context.ReplyToCommandAsync(embed: embed.BuildSync());
                }
                else
                {
                    await StopAsync();
                }
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
            }
        }

        [SlashCommand("leave", "Leave the current voice channel.")]
        public async Task LeaveAsync()
        {
            var embed = EmbedHandler.Create(Context);

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);

                if (player != null)
                {
                    await player.DisconnectAsync();
                }

                embed.AddField("ZirconSound Left", "ZirconSound left the voice channel.");
                await Context.ReplyToCommandAsync(embed: embed.BuildSync());
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
            }
        }

        [SlashCommand("pause", "Pause the current track.")]
        [MessageComponent("pause-button")]
        public async Task PauseAsync()
        {
            var embed = EmbedHandler.Create(Context);

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);

                if (player is { State: PlayerState.Playing })
                {
                    await player.PauseAsync();

                    var button = new ComponentBuilder().WithButton("Resume", "resume-button");

                    embed.AddField("Paused", "Paused the current track.");
                    await Context.ReplyToCommandAsync(embed: embed.BuildSync(), component: button.Build());
                    await _playerService.InitiateDisconnectAsync(player, TimeSpan.FromMinutes(15));

                }
                else if (player is { State: PlayerState.Paused })
                {
                    embed.AddField("Pause", "The track is already paused.");
                    await Context.ReplyToCommandAsync(embed: embed.BuildSync());
                }
                else
                {
                    embed.AddField("Warning", "Unabled to pause the track.");
                    await Context.ReplyToCommandAsync(embed: embed.BuildSync(ZirconEmbedType.Warning));
                }
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
            }
        }
        
        [SlashCommand("resume", "Resume the track if the track is paused.")]
        [MessageComponent("resume-button")]
        public async Task ResumeAsync()
        {
            var embed = EmbedHandler.Create(Context);

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);

                if (player != null)
                {
                    switch (player.State)
                    {
                        case PlayerState.Paused:
                            await player.ResumeAsync();

                            var button = new ComponentBuilder().WithButton("Pause", "pause-button", ButtonStyle.Danger);

                            embed.AddField("Resumed", "Resumed the current track.");
                            await Context.ReplyToCommandAsync(embed: embed.BuildSync(), component: button.Build());
                            await _playerService.CancelDisconnectAsync(player);
                            break;
                        case PlayerState.Playing:
                            embed.AddField("Playing", "Track is already playing.");
                            await Context.ReplyToCommandAsync(embed: embed.BuildSync());
                            break;
                        case PlayerState.NotPlaying:
                            break;
                        case PlayerState.Destroyed:
                            break;
                        case PlayerState.NotConnected:
                            break;
                        default:
                            embed.AddField("Warning", "Unabled to resume the track.");
                            await Context.ReplyToCommandAsync(embed: embed.BuildSync(ZirconEmbedType.Warning));
                            break;
                    }
                }
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
            }
        }

        [SlashCommand("queue", "Get the queue lenght and list of tracks", "page", ApplicationCommandOptionType.Integer, "the page number", false)]
        [MessageComponent("queue-button")]
        public async Task QueueAsync(long pageNum)
        {
            var embed = EmbedHandler.Create(Context);
            var page = (int)pageNum;
            if (page - 1 < 0)
            {
                page = 0;
            }
            else
            {
                page--;
            }

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);

                if (player != null && player.Queue.Tracks.Count > 0)
                {
                    var tracks = player.Queue.Tracks.ToList();
                    var tracksChunk = tracks.ChunkBy(5);

                    if (page + 1 > tracksChunk.Count)
                    {
                        page = tracksChunk.Count - 1;
                    }

                    var songList = string.Empty;
                    var i = page * 5;
                    var estimatedTime = TimeSpan.FromSeconds(0);

                    foreach (var track in tracksChunk[page])
                    {
                        i++;
                        songList += $"{i}- [{track.Title}]({track.Source})\n";
                    }

                    estimatedTime = tracks.Aggregate(estimatedTime, (current, track) => current + track.Duration);

                    embed.AddField("Queue", songList);
                    if (tracksChunk.Count > 1)
                    {
                        embed.AddField(new EmbedFieldBuilder().WithName("Pages").WithValue($"{page + 1} of {tracksChunk.Count}").WithIsInline(true));
                    }

                    var firstDisabled = false;
                    var lastDisabled = false;

                    if (page == 0)
                    {
                        firstDisabled = true;
                    }

                    if (page == tracksChunk.Count - 1)
                    {
                        lastDisabled = true;
                    }

                    var button = new ComponentBuilder().WithButton("First", "queue-button", ButtonStyle.Secondary, disabled: firstDisabled)
                        .WithButton("Back", $"queue-button;{page}", ButtonStyle.Secondary, disabled: firstDisabled)
                        .WithButton("Clear", "clear-button")
                        .WithButton("Next", $"queue-button;{page + 2}", ButtonStyle.Secondary, disabled: lastDisabled)
                        .WithButton("Last", $"queue-button;{tracksChunk.Count}", ButtonStyle.Secondary, disabled: lastDisabled);

                    embed.AddField(new EmbedFieldBuilder().WithName("Estimated play time").WithValue(estimatedTime).WithIsInline(true));
                    await Context.ReplyToCommandAsync(embed: embed.BuildSync(), component: button.Build());
                    return;
                }

                embed.AddField("Empty", "The queue is empty!");
                await Context.ReplyToCommandAsync(embed: embed.BuildSync());
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
            }
        }

        [SlashCommand("clear", "Clear the playlist.")]
        [MessageComponent("clear-button")]
        public async Task ClearAsync()
        {
            var embed = EmbedHandler.Create(Context);

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);

                if (player != null && player.Queue.Tracks.Count > 0)
                {
                    player.Queue.Clear();

                    var button = new ComponentBuilder();
                    embed.AddField("Cleared", "Cleared the queue!");
                    await Context.ReplyToCommandAsync(embed: embed.BuildSync(), component: button.Build());
                    return;
                }

                embed.AddField("Empty", "The queue is empty!");
                await Context.ReplyToCommandAsync(embed: embed.BuildSync());
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: embed.BuildSync(ZirconEmbedType.Warning));
            }
        }

        [SlashCommand("seek", "Seek the current track to the specified value. (00:00:00) (Hours:Minutes:Seconds)", "time", ApplicationCommandOptionType.String, "(00:00:00) (Hours:Minutes:Seconds)", true)]
        public async Task SeekAsync(string seekTime)
        {
            var embed = EmbedHandler.Create(Context);

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);

                if (player is { State: PlayerState.Playing })
                {
                    _ = TimeSpan.TryParse(seekTime, out var timeSeek);

                    if (timeSeek >= TimeSpan.Zero)
                    {
                        if (player.CurrentTrack != null && timeSeek <= player.CurrentTrack.Duration)
                        {
                            await player.SeekPositionAsync(timeSeek);

                            embed.AddField("Seeked", $"Seeked to: {timeSeek}");
                            await Context.ReplyToCommandAsync(embed: embed.BuildSync());
                            return;
                        }

                        embed.AddField("Out of range", "You can't seek to a time higher than the duration of the track!");
                        await Context.ReplyToCommandAsync(embed: embed.BuildSync());
                        return;
                    }

                    embed.AddField("Negative", "You can't seek to a negative number!");
                    await Context.ReplyToCommandAsync(embed: embed.BuildSync());
                    return;
                }

                embed.AddField("Not playing", "No track is playing...");
                await Context.ReplyToCommandAsync(embed: embed.BuildSync());
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
            }
        }

        [SlashCommand("replay", "Replay the current track.")]
        public async Task ReplayAsync()
        {
            var embed = EmbedHandler.Create(Context);

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);

                if (player != null && player.State != PlayerState.NotConnected)
                {
                    await _playerService.CancelDisconnectAsync(player);
                    await player.ReplayAsync();

                    embed.AddField("Replaying", "Replaying current track!");
                    await Context.ReplyToCommandAsync(embed: embed.BuildSync());
                    return;
                }

                embed.AddField("Not playing", "No track is playing...");
                await Context.ReplyToCommandAsync(embed: embed.BuildSync());
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
            }
        }

        [SlashCommand("loop", "Set or unset the player into loop mode.")]
        [MessageComponent("loop-button")]
        public async Task LoopAsync()
        {
            var embed = EmbedHandler.Create(Context);

            if (CheckState(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = _audioService.GetPlayer<ZirconPlayer>(Context.Guild.Id);

                if (player != null && player.State != PlayerState.NotConnected)
                {
                    if (!player.IsLooping)
                    {
                        player.IsLooping = true;

                        var button = new ComponentBuilder().WithButton("Unloop", "loop-button", ButtonStyle.Secondary);

                        embed.AddField("Looping", "The player is now looping tracks!");
                        await Context.ReplyToCommandAsync(embed: embed.BuildSync(), component: button.Build());

                    }
                    else
                    {
                        player.IsLooping = false;

                        var button = new ComponentBuilder().WithButton("Loop", "loop-button", ButtonStyle.Secondary);

                        embed.AddField("Looping", "The player not looping tracks anymore!");
                        await Context.ReplyToCommandAsync(embed: embed.BuildSync(), component: button.Build());
                    }
                    return;
                }
                embed.AddField("Not playing", "No track is playing...");
                await Context.ReplyToCommandAsync(embed: embed.BuildSync());
            }
            else
            {
                await Context.ReplyToCommandAsync(embed: _errorEmbed.BuildSync(ZirconEmbedType.Warning));
            }
        }
    }
}