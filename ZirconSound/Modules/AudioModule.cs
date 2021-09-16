using Discord;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using ZirconSound.Services;

namespace ZirconSound.Modules
{
    public sealed class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private readonly AudioService _audioService;

        public AudioModule(LavaNode lavaNode, AudioService audioService)
        {
            _lavaNode = audioService.LavaNode;
            _audioService = audioService;


        }

        [Command("play")]
        public async Task PlayAsync([Remainder] string query)
        {
            if (!_lavaNode.IsConnected) 
            {
                await _lavaNode.ConnectAsync();
            }
            if (string.IsNullOrWhiteSpace(query))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await JoinAsync(true);
            }

            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.Status == Victoria.Responses.Search.SearchStatus.LoadFailed ||
                searchResponse.Status == Victoria.Responses.Search.SearchStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    foreach (var track in searchResponse.Tracks)
                    {
                        player.Queue.Enqueue(track);
                    }

                    await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    var track = searchResponse.Tracks.ToList()[0];
                    player.Queue.Enqueue(track);

                    var embed = new EmbedBuilder
                    {
                        Title = "Added to queue:",
                        Description = $"[{track.Title}]({track.Url})",
                        Color = Color.DarkBlue,
                        Timestamp = DateTime.Now,
                    };

                    await ReplyAsync(embed: embed.Build());
                }
            }
            else
            {
                var track = searchResponse.Tracks.ToList()[0];

                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    for (var i = 0; i < searchResponse.Tracks.Count; i++)
                    {
                        if (i == 0)
                        {
                            await player.PlayAsync(track);
                            var embed = new EmbedBuilder
                            {
                                Title = "Now Playing:",
                                Description = $"[{track.Title}]({track.Url})",
                                Color = Color.DarkBlue,
                                Timestamp = DateTime.Now,
                            };
                            await ReplyAsync(embed: embed.Build());
                            await ReplyAsync($"{track.Url}");
                        }
                        else
                        {
                            player.Queue.Enqueue(searchResponse.Tracks.ToList()[i]);
                        }
                    }

                    await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    await player.PlayAsync(track);
                    var embed = new EmbedBuilder
                    {
                        Title = "Now Playing:",
                        Description = $"[{track.Title}]({track.Url})",
                        Color = Color.DarkBlue,
                        Timestamp = DateTime.Now,
                    };
                    await ReplyAsync(embed: embed.Build());
                }
            }
        }

        [Command("join")]
        public async Task JoinAsync(bool dontDisconnect = false)
        {
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
                if (!dontDisconnect)
                {
                    await _audioService.InitiateDisconnectAsync(_lavaNode.GetPlayer(Context.Guild), TimeSpan.FromSeconds(120));
                }
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("skip")]
        public async Task SkipAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me!");
                return;
            }

            if (player.Queue.Count == 0)
            {
                await ReplyAsync("There are no more song in the queue!");
                return;
            }


            await player.SkipAsync();
            await ReplyAsync($"Skipped! Now playing: **{player.Track.Title}**\n{player.Track.Url}");
        }

        [Command("pause")]
        public async Task PauseAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me!");
                return;
            }

            if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
            {
                await ReplyAsync("The music is already paused!");
                return;
            }


            await player.PauseAsync();
            await ReplyAsync($"Paused the music!");
        }

        [Command("resume")]
        public async Task ResumeAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me!");
                return;
            }

            if (player.PlayerState == PlayerState.Playing)
            {
                await ReplyAsync("The music is already playing!");
                return;
            }


            await player.ResumeAsync();
            await ReplyAsync($"Resumed the music!");
        }

        [Command("queue")]
        public async Task QueueAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me!");
                return;
            }

            if (player.Queue.Count == 0)
            {
                await ReplyAsync("There are no more song in the queue!");
                return;
            }

            int i = 0;
            string queueString = "";

            foreach (var item in player.Queue) 
            {
                i++;
                queueString += $"\n{i} - [{item.Title}]({item.Url})";
            }
            var embed = new EmbedBuilder
            {
                Title = "Queue",
                Description = $"Current playing music:\n[{player.Track.Title}]({player.Track.Url})"
            };
            embed.AddField("Current music in queue",
                queueString)
                .WithAuthor(Context.Client.CurrentUser)
                .WithColor(Color.DarkBlue)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("clear")]
        public async Task ClearAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me!");
                return;
            }

            if (player.Queue.Count == 0)
            {
                await ReplyAsync("There are no more song in the queue!");
                return;
            }

            player.Queue.Clear();
            await ReplyAsync($"Cleared the current queue!");
        }

        [Command("stop")]
        public async Task StopAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me!");
                return;
            }

            if (player.Track == null)
            {
                await ReplyAsync("There is no song playing!");
                return;
            }


            await player.StopAsync();
            await ReplyAsync($"Stopped the current song!");
        }

        [Command("leave")]
        public async Task LeaveAsync()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                
                await ReplyAsync($"Left the voice channel!");
                await _audioService.InitiateDisconnectAsync(_lavaNode.GetPlayer(Context.Guild), TimeSpan.FromSeconds(0));
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }
    }
}
