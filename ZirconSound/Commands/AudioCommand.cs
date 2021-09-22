using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZirconSound.DiscordHandlers;
using ZirconSound.Enum;
using ZirconSound.Player;
using ZirconSound.Services;
using ZirconSound.Extensions;

namespace ZirconSound.Commands
{
    public class AudioCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IAudioService AudioService;
        private readonly EmbedHandler EmbedHandler;
        private readonly PlayerService playerService;

        public AudioCommand(IAudioService audioService, EmbedHandler embedHandler, PlayerService iplayerService)
        {
            AudioService = audioService;
            EmbedHandler = embedHandler;
            playerService = iplayerService;
        }

        private async Task<bool> CheckStateAsync(IEnumerable<AudioState> audioStates, SocketCommandContext context, IUserMessage msg = null)
        {
            var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(context.Guild.Id);
            var voiceState = Context.User as IVoiceState;
            var voiceChannel = voiceState.VoiceChannel;

            var embed = EmbedHandler.Create();

            if (audioStates.Contains(AudioState.BotIsInVoiceChannel))
            {
                if (player == null)
                {
                    if (msg != null)
                    {
                        embed.AddField("Warning:", "Is not in a voice channel!");
                        await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                        return false;
                    }
                }
            }

            if (audioStates.Contains(AudioState.BotIsNotInVoiceChannel))
            {
                if (player != null)
                {
                    if (msg != null)
                    {
                        embed.AddField("Warning:", "Bot is already in a voice channel!");
                        await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                        return false;
                    }
                }
            }

            if (audioStates.Contains(AudioState.UserIsInVoiceChannel))
            {
                if (voiceChannel == null)
                {
                    if (msg != null)
                    {
                        embed.AddField("Warning:", "You are not in a voice channel!");
                        await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                        return false;
                    }
                }
            }

            if (audioStates.Contains(AudioState.UserIsNotInVoiceChannel))
            {
                if (voiceChannel != null)
                {
                    if (msg != null)
                    {
                        embed.AddField("Warning:", "You are already in a voice channel!");
                        await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                        return false;
                    }
                }
            }

            if (audioStates.Contains(AudioState.BotAndUserInSameVoiceChannel))
            {
                if (voiceChannel.Id != player.VoiceChannelId)
                {
                    if (msg != null)
                    {
                        embed.AddField("Warning:", "You need to be in the same voice channel!");
                        await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                        return false;
                    }
                }
            }

            if (audioStates.Contains(AudioState.BotAndUserNotInSameVoiceChannel))
            {
                if (voiceChannel.Id != player.VoiceChannelId)
                {
                    if (msg != null)
                    {
                        embed.AddField("Warning:", "You need to be in a different voice channel!");
                        await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                        return false;
                    }
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

        [Command("play")]
        [Alias("p")]
        public async Task PlayAsync([Remainder] string id)
        {

            var embed = EmbedHandler.Create();
            embed.AddField("Searching", "Searching for current song.");
            var msg = await Context.Channel.SendMessageAsync(embed: embed.BuildSync());
            if (await CheckStateAsync(new List<AudioState> { AudioState.BotIsNotInVoiceChannel }, Context))
            {
                if (await CheckStateAsync(new List<AudioState> { AudioState.UserIsInVoiceChannel }, Context, msg))
                {
                    await JoinAsync();
                }
            }

            var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context, msg))
            {
                var tracks = await AudioService.GetTracksAsync(id);
                if (!tracks.Any())
                {
                    var track = await AudioService.GetTrackAsync(id, Lavalink4NET.Rest.SearchMode.YouTube, true);
                    tracks = new List<LavalinkTrack> { track };
                }

                if (tracks.FirstOrDefault() != null)
                {
                    embed = EmbedHandler.Create();

                    if (player.CurrentTrack == null)
                    {
                        await playerService.CancelDisconnectAsync(player);
                        var track = tracks.First();
                        embed.AddField("Playing:", $"[{track.Title}]({track.Source})");
                        EmbedSong(ref embed, track);
                        tracks.ToList().RemoveAt(0);
                        await player.PlayAsync(track);
                    }
                    else
                    {
                        if (tracks.Count() <= 1)
                        {
                            var track = tracks.First();
                            embed.AddField("Queued:", $"[{track.Title}]({track.Source})");
                            EmbedSong(ref embed, track);

                            var timeLeft = TimeSpan.FromSeconds(0);
                            foreach (var trackQueue in player.Queue.Tracks)
                            {
                                timeLeft += trackQueue.Duration;
                            }
                            timeLeft += player.CurrentTrack.Duration - player.CurrentTrack.Position;

                            var estimatedTime = new EmbedFieldBuilder().WithName("Estimated time until song").WithValue(timeLeft).WithIsInline(true);

                            embed.AddField(estimatedTime);
                            embed.AddField("Position in queue", player.Queue.Tracks.Count + 1);
                            tracks.ToList().RemoveAt(0);
                            player.Queue.Add(track);
                        }
                    }

                    if (tracks.Count() > 1)
                    {
                        var estimatedTime = new EmbedFieldBuilder().WithName("Queued:").WithValue($"{tracks.Count()} song!").WithIsInline(true);

                        embed.AddField(estimatedTime);

                        var timeLeft = TimeSpan.FromSeconds(0);
                        foreach (var track in tracks)
                        {
                            timeLeft += track.Duration;
                            player.Queue.Add(track);
                        }
                        embed.AddField("Estimated play time:", $"{timeLeft}");

                    }
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                embed = EmbedHandler.Create();
                embed.AddField("Warning:", "Unable to find the specified song!");
                await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
            }
        }

        [Command("stop")]
        public async Task StopAsync()
        {
            var embed = EmbedHandler.Create();
            embed.AddField("Stopping", "Stopping current track.");
            var msg = await Context.Channel.SendMessageAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context, msg))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);
                if (player.State == PlayerState.Playing || player.State == PlayerState.Paused)
                {

                    embed = EmbedHandler.Create();
                    embed.AddField("Stopped:", $"Stopped current track!");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());

                    await player.StopAsync();
                    return;
                }
                else
                {
                    embed = EmbedHandler.Create();
                    embed.AddField("Unable to stop:", $"No track are actually playing couln't stop the track!");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                }
            }
        }

        [Command("skip")]
        [Alias("s, next")]
        public async Task SkipAsync()
        {
            var embed = EmbedHandler.Create();
            embed.AddField("Skipping", "Skipping current track.");
            var msg = await Context.Channel.SendMessageAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context, msg))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.Queue.Count > 0)
                {
                    var track = player.Queue.FirstOrDefault();

                    embed = EmbedHandler.Create();
                    embed.AddField("Skipped now playing:", $"[{track.Title}]({track.Source})");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());

                    await player.SkipAsync();
                    return;
                }
                else
                {
                    await msg.DeleteAsync();
                    await StopAsync();
                    return;
                }
            }

            embed = EmbedHandler.Create();
            embed.AddField("Error:", $"couldn't skip track");
            await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
        }

        [Command("leave")]
        public async Task LeaveAsync()
        {
            var embed = EmbedHandler.Create();
            embed.AddField("Leaving", "Leaving the current channel.");
            var msg = await Context.Channel.SendMessageAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context, msg))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                await player.DisconnectAsync();

                embed = EmbedHandler.Create();
                embed.AddField("Left", "Left the channel.");
                await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                return;
            }
        }

        [Command("pause")]
        public async Task PauseAsync()
        {
            var embed = EmbedHandler.Create();
            embed.AddField("Pausing", "Pausing the current song.");
            var msg = await Context.Channel.SendMessageAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context, msg))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.State == PlayerState.Playing)
                {
                    await player.PauseAsync();

                    embed = EmbedHandler.Create();
                    embed.AddField("Paused", "Paused the current song.");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                else if (player.State == PlayerState.Paused)
                {
                    embed = EmbedHandler.Create();
                    embed.AddField("Pause", "The song is already paused.");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                else
                {
                    embed = EmbedHandler.Create();
                    embed.AddField("Warning", "Unabled to pause the song.");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                    return;
                }
            }
        }

        [Command("resume")]
        public async Task ResumeAsync()
        {
            var embed = EmbedHandler.Create();
            embed.AddField("Resuming", "Resuming the current song.");
            var msg = await Context.Channel.SendMessageAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context, msg))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.State == PlayerState.Paused)
                {
                    await player.ResumeAsync();

                    embed = EmbedHandler.Create();
                    embed.AddField("Resumed", "Resumed the current song.");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                else if (player.State == PlayerState.Playing)
                {
                    embed = EmbedHandler.Create();
                    embed.AddField("Playing", "Song is already playing.");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                else
                {
                    embed = EmbedHandler.Create();
                    embed.AddField("Warning", "Unabled to resume the song.");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync(ZirconEmbedType.Warning));
                    return;
                }
            }
        }

        [Command("queue")]
        public async Task QueueAsync(int page = 0)
        {
            var embed = EmbedHandler.Create();
            embed.AddField("Getting queue.", "Getting current queue.");
            var msg = await Context.Channel.SendMessageAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context, msg))
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

                    embed = EmbedHandler.Create();
                    embed.AddField("Queue:", songList);
                    if (tracksChunk.Count > 1)
                    {
                        embed.AddField(new EmbedFieldBuilder().WithName("Pages").WithValue($"{ page + 1} of {tracksChunk.Count + 1}").WithIsInline(true));
                    }
                    embed.AddField(new EmbedFieldBuilder().WithName("Estimated play time:").WithValue(estimatedTime).WithIsInline(true));
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                embed = EmbedHandler.Create();
                embed.AddField("Empty", "The queue is empty!");
                await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                return;
            }
        }

        [Command("clear")]
        public async Task ClearAsync()
        {
            var embed = EmbedHandler.Create();
            embed.AddField("Clearing Queue.", "Clearing the queue.");
            var msg = await Context.Channel.SendMessageAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context, msg))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.Queue.Tracks.Count > 0)
                {
                    player.Queue.Clear();

                    embed = EmbedHandler.Create();
                    embed.AddField("Cleared", "Cleared the queue!");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                embed = EmbedHandler.Create();
                embed.AddField("Empty", "The queue is empty!");
                await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                return;
            }
        }

        [Command("seek")]
        public async Task SeekAsync(string seekTime = "0")
        {
            var embed = EmbedHandler.Create();
            embed.AddField("Seeking.", "Seeking to the specified time.");
            var msg = await Context.Channel.SendMessageAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context, msg))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.State == PlayerState.Playing)
                {
                    TimeSpan.TryParse(seekTime, out var timeSeek);

                    if (timeSeek >= TimeSpan.Zero)
                    {
                        if (timeSeek <= player.CurrentTrack.Duration)
                        {
                            await player.SeekPositionAsync(timeSeek);

                            embed = EmbedHandler.Create();
                            embed.AddField("Seeked", $"Seeked to: {timeSeek}");
                            await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                            return;
                        }
                        embed = EmbedHandler.Create();
                        embed.AddField("Out of range", "You can't seek to a time higher than the duration of the song!");
                        await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                        return;
                    }
                    embed = EmbedHandler.Create();
                    embed.AddField("Negative", "You can't seek to a negative number!");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                embed = EmbedHandler.Create();
                embed.AddField("Not playing", "No song is playing...");
                await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                return;
            }
        }

        [Command("replay")]
        public async Task ReplayAsync()
        {
            var embed = EmbedHandler.Create();
            embed.AddField("Replaying.", "Replaying song.");
            var msg = await Context.Channel.SendMessageAsync(embed: embed.BuildSync());

            if (await CheckStateAsync(new List<AudioState>
            {
                AudioState.UserIsInVoiceChannel,
                AudioState.BotIsInVoiceChannel,
                AudioState.BotAndUserInSameVoiceChannel
            }, Context, msg))
            {
                var player = AudioService.GetPlayer<QueuedLavalinkPlayer>(Context.Guild.Id);

                if (player.State != PlayerState.NotConnected)
                {
                    await playerService.CancelDisconnectAsync(player);
                    await player.ReplayAsync();

                    embed = EmbedHandler.Create();
                    embed.AddField("Replaying", "Replaying current track!");
                    await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                    return;
                }
                embed = EmbedHandler.Create();
                embed.AddField("Not playing", "No song is playing...");
                await msg.ModifyAsync(msg => msg.Embed = embed.BuildSync());
                return;
            }
        }
    }
}

