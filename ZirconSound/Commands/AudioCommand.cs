using Discord;
using Discord.Commands;
using Lavalink4NET;
using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZirconSound.DiscordHandlers;
using ZirconSound.Enum;
using ZirconSound.Extensions;
using ZirconSound.Player;
using ZirconSound.SlashCommands;

namespace ZirconSound.Commands
{
    public class AudioCommand : SlashModuleBase<SlashCommandContext>
    {
        private readonly IAudioService AudioService;
        private readonly PlayerService playerService;

        public AudioCommand(IAudioService audioService, PlayerService iplayerService)
        {
            AudioService = audioService;
            playerService = iplayerService;
        }

        private async Task<bool> CheckStateAsync(IEnumerable<AudioState> audioStates, ICommandContext context)
        {
            var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(context.Guild.Id);
            var voiceState = Context.User as IVoiceState;
            var voiceChannel = voiceState.VoiceChannel;

            var embed = EmbedHandler.Create(Context);

            if (audioStates.Contains(AudioState.BotIsInVoiceChannel))
            {
                if (player == null)
                {
                    embed.AddField("Warning:", "Is not in a voice channel!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                    return false;
                }
            }

            if (audioStates.Contains(AudioState.BotIsNotInVoiceChannel))
            {
                if (player != null)
                {
                    embed.AddField("Warning:", "Bot is already in a voice channel!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                    return false;
                }
            }

            if (audioStates.Contains(AudioState.UserIsInVoiceChannel))
            {
                if (voiceChannel == null)
                {
                    embed.AddField("Warning:", "You are not in a voice channel!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                    return false;
                }
            }

            if (audioStates.Contains(AudioState.UserIsNotInVoiceChannel))
            {
                if (voiceChannel != null)
                {
                    embed.AddField("Warning:", "You are already in a voice channel!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                    return false;
                }
            }

            if (audioStates.Contains(AudioState.BotAndUserInSameVoiceChannel))
            {
                if (voiceChannel.Id != player.VoiceChannelId)
                {
                    embed.AddField("Warning:", "You need to be in the same voice channel!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                    return false;
                }
            }

            if (audioStates.Contains(AudioState.BotAndUserNotInSameVoiceChannel))
            {
                if (voiceChannel.Id != player.VoiceChannelId)
                {
                    embed.AddField("Warning:", "You need to be in a different voice channel!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                    return false;
                }
            }

            return true;
        }

        public async Task JoinAsync()
        {
            var voiceState = Context.User as IVoiceState;
            var voiceChannel = voiceState.VoiceChannel;
            await AudioService.JoinAsync<QueuedLavalinkPlayer>(Context.Guild.Id, voiceChannel.Id, true);
        }

        private static ZirconEmbed EmbedSong(ref ZirconEmbed embedBuilder, LavalinkTrack lavalinkTrack)
        {
            var channel = new EmbedFieldBuilder().WithName("Channel").WithValue(lavalinkTrack.Author).WithIsInline(true);
            var duration = new EmbedFieldBuilder().WithName("Duration").WithValue(lavalinkTrack.Duration).WithIsInline(true);


            embedBuilder.WithThumbnailUrl($"https://img.youtube.com/vi/{lavalinkTrack.TrackIdentifier}/0.jpg");
            embedBuilder.AddField(channel);
            embedBuilder.AddField(duration);

            return embedBuilder;
        }

        [SlashCommand("play", "Play the song", "song", ApplicationCommandOptionType.String, "Id/Name/Link/Playlist", true)]
        public async Task PlayAsync([Remainder] string id)
        {
            if (await CheckStateAsync(new List<AudioState> { AudioState.BotIsNotInVoiceChannel }, Context))
            {
                if (await CheckStateAsync(new List<AudioState> { AudioState.UserIsInVoiceChannel }, Context))
                {
                    await JoinAsync();
                }
            }

            var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var tracks = await AudioService.GetTracksAsync(id);
                if (!tracks.Any())
                {
                    var track = await AudioService.GetTrackAsync(id, Lavalink4NET.Rest.SearchMode.YouTube, true);
                    tracks = new List<LavalinkTrack> { track };
                }

                if (tracks.FirstOrDefault() != null)
                {
                    var embed = EmbedHandler.Create(Context);

                    if (player.CurrentTrack == null)
                    {
                        await playerService.CancelDisconnectAsync(player);
                        var track = tracks.First();
                        embed.AddField("Playing:", $"[{track.Title}]({track.Source})");
                        EmbedSong(ref embed, track);
                        tracks = tracks.Skip(1);
                        await player.PlayAsync(track);
                    }
                    else
                    {
                        if (tracks.Count() <= 1)
                        {
                            var track = tracks.First();
                            tracks = tracks.Skip(1);

                            embed.AddField("Queued:", $"[{track.Title}]({track.Source})");
                            EmbedSong(ref embed, track);

                            var timeLeft = TimeSpan.FromSeconds(0);
                            foreach (var trackQueue in player.Queue.Tracks)
                            {
                                timeLeft += trackQueue.Duration;
                            }
                            timeLeft += (player.CurrentTrack.Duration - player.CurrentTrack.Position);

                            var estimatedTime = new EmbedFieldBuilder().WithName("Estimated time until track").WithValue(timeLeft).WithIsInline(true);

                            embed.AddField(estimatedTime);
                            embed.AddField("Position in queue", player.Queue.Tracks.Count + 1);
                            player.Queue.Add(track);
                        }
                    }

                    if (tracks.Count() > 1)
                    {
                        var estimatedTime = new EmbedFieldBuilder().WithName("Queued:").WithValue($"{tracks.Count()} tracks!").WithIsInline(true);

                        embed.AddField(estimatedTime);

                        var timeLeft = TimeSpan.FromSeconds(0);
                        foreach (var track in tracks)
                        {
                            timeLeft += track.Duration;
                            player.Queue.Add(track);
                        }
                        embed.AddField("Estimated play time:", $"{timeLeft}");

                    }
                    await Context.Command.FollowupAsync(embed: embed.BuildSync());
                    return;
                }
                var embeder = EmbedHandler.Create(Context);
                embeder.AddField("Warning:", "Unable to find the specified track!");
                await Context.Command.FollowupAsync(embed: embeder.BuildSync(ZirconEmbedType.Warning), ephemeral: false);
            }
        }

        [SlashCommand("stop", "Stop the current song")]
        public async Task StopAsync()
        {
            var embed = EmbedHandler.Create(Context);
            embed.AddField("Stopping", "Stopping current track.");
            var msg = await Context.Command.FollowupAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);
                if (player.State == PlayerState.Playing || player.State == PlayerState.Paused)
                {

                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Stopped", $"Stopped current track!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());

                    await player.StopAsync();
                    return;
                }
                else
                {
                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Unable to stop:", $"No track are actually playing couln't stop the track!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                }
            }
        }

        [SlashCommand("skip", "Skip the current song")]
        public async Task SkipAsync()
        {
            var embed = EmbedHandler.Create(Context);
            embed.AddField("Skipping", "Skipping current track.");
            var msg = await Context.Command.FollowupAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.Queue.Count > 0)
                {
                    var track = player.Queue.FirstOrDefault();

                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Skipped now playing:", $"[{track.Title}]({track.Source})");
                    EmbedSong(ref embed, track);

                    await player.SkipAsync();

                    if (player.Queue.Count > 1)
                    {
                        var queueLength = new EmbedFieldBuilder().WithName("Queue count").WithValue($"{player.Queue.Count} tracks").WithIsInline(true);
                        embed.AddField(queueLength);
                    }

                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                else
                {
                    await msg.DeleteAsync();
                    await StopAsync();
                    return;
                }
            }

            embed = EmbedHandler.Create(Context);
            embed.AddField("Error:", $"couldn't skip track");
            await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
        }

        [SlashCommand("leave", "Leave the current channel")]
        public async Task LeaveAsync()
        {
            var embed = EmbedHandler.Create(Context);
            embed.AddField("Leaving", "Leaving the current channel.");
            var msg = await Context.Command.FollowupAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                await player.DisconnectAsync();

                embed = EmbedHandler.Create(Context);
                embed.AddField("Left", "Left the channel.");
                await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                return;
            }
        }

        [SlashCommand("pause", "Pause the current song.")]
        public async Task PauseAsync()
        {
            var embed = EmbedHandler.Create(Context);
            embed.AddField("Pausing", "Pausing the current track.");
            var msg = await Context.Command.FollowupAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.State == PlayerState.Playing)
                {
                    await player.PauseAsync();

                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Paused", "Paused the current track.");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                else if (player.State == PlayerState.Paused)
                {
                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Pause", "The track is already paused.");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                else
                {
                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Warning", "Unabled to pause the track.");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                    return;
                }
            }
        }

        [SlashCommand("resume", "Resume the track if the track is paused.")]
        public async Task ResumeAsync()
        {
            var embed = EmbedHandler.Create(Context);
            embed.AddField("Resuming", "Resuming the current track.");
            var msg = await Context.Command.FollowupAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.State == PlayerState.Paused)
                {
                    await player.ResumeAsync();

                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Resumed", "Resumed the current track.");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                else if (player.State == PlayerState.Playing)
                {
                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Playing", "Track is already playing.");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                else
                {
                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Warning", "Unabled to resume the track.");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                    return;
                }
            }
        }

        [SlashCommand("queue", "Get the queue lenght and list of tracks", "page", ApplicationCommandOptionType.Integer, "Id/Name/Link/Playlist", false)]
        public async Task QueueAsync(int page = 0)
        {
            var embed = EmbedHandler.Create(Context);
            embed.AddField("Getting queue.", "Getting current queue.");
            var msg = await Context.Command.FollowupAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.Queue.Tracks.Count > 0)
                {
                    var tracks = player.Queue.Tracks.ToList();
                    var tracksChunk = tracks.ChunkBy(5);
                    var songList = string.Empty;
                    var i = page * 5;
                    var estimatedTime = TimeSpan.FromSeconds(0);

                    foreach (var track in tracksChunk[page])
                    {
                        i++;
                        songList += $"{i}- [{track.Title}]({track.Source})\n";
                    }

                    foreach (var track in tracks)
                    {
                        estimatedTime += track.Duration;
                    }

                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Queue", songList);
                    if (tracksChunk.Count > 1)
                    {
                        embed.AddField(new EmbedFieldBuilder().WithName("Pages").WithValue($"{ page + 1} of {tracksChunk.Count + 1}").WithIsInline(true));
                    }
                    embed.AddField(new EmbedFieldBuilder().WithName("Estimated play time").WithValue(estimatedTime).WithIsInline(true));
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                embed = EmbedHandler.Create(Context);
                embed.AddField("Empty", "The queue is empty!");
                await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                return;
            }
        }

        [SlashCommand("clear", "Clear the playlist.")]
        public async Task ClearAsync()
        {
            var embed = EmbedHandler.Create(Context);
            embed.AddField("Clearing Queue.", "Clearing the queue.");
            var msg = await Context.Command.FollowupAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.Queue.Tracks.Count > 0)
                {
                    player.Queue.Clear();

                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Cleared", "Cleared the queue!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                embed = EmbedHandler.Create(Context);
                embed.AddField("Empty", "The queue is empty!");
                await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                return;
            }
        }

        [SlashCommand("seek", "Seek the current track to the specified value", "time", ApplicationCommandOptionType.String, "(00:00:00) (Hours:Minutes:Seconds)", false)]
        public async Task SeekAsync(string seekTime = "0")
        {
            var embed = EmbedHandler.Create(Context);
            embed.AddField("Seeking.", "Seeking to the specified time.");
            var msg = await Context.Command.FollowupAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.State == PlayerState.Playing)
                {
                    _ = TimeSpan.TryParse(seekTime, out var timeSeek);

                    if (timeSeek >= TimeSpan.Zero)
                    {
                        if (timeSeek <= player.CurrentTrack.Duration)
                        {
                            await player.SeekPositionAsync(timeSeek);

                            embed = EmbedHandler.Create(Context);
                            embed.AddField("Seeked", $"Seeked to: {timeSeek}");
                            await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                            return;
                        }
                        embed = EmbedHandler.Create(Context);
                        embed.AddField("Out of range", "You can't seek to a time higher than the duration of the track!");
                        await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                        return;
                    }
                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Negative", "You can't seek to a negative number!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                embed = EmbedHandler.Create(Context);
                embed.AddField("Not playing", "No track is playing...");
                await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                return;
            }
        }

        [SlashCommand("replay", "Replay the current track.")]
        public async Task ReplayAsync()
        {
            var embed = EmbedHandler.Create(Context);
            embed.AddField("Replaying.", "Replaying track.");
            var msg = await Context.Command.FollowupAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.State != PlayerState.NotConnected)
                {
                    await playerService.CancelDisconnectAsync(player);
                    await player.ReplayAsync();

                    embed = EmbedHandler.Create(Context);
                    embed.AddField("Replaying", "Replaying current track!");
                    await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                embed = EmbedHandler.Create(Context);
                embed.AddField("Not playing", "No track is playing...");
                await Context.Command.ModifyOriginalResponseAsync(msg => msg.Embed = embed.BuildSync());
                return;
            }
        }
    }
}

