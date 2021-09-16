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
using ZirconSound.Common;
using ZirconSound.Services;

namespace ZirconSound.Modules
{
    public sealed class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private readonly AudioService _audioService;

        public AudioModule(LavaNode lavaNode, AudioService audioService)
        {
            _lavaNode = lavaNode;
            _audioService = audioService;


        }

        [Command("play")]
        public async Task PlayAsync([Remainder] string query)
        {
            var zirconEmbed = new ZirconEmbedBuilder();
            zirconEmbed.AddField("Searching", "Searching for current music...");
            var msg = await ReplyAsync(embed: zirconEmbed.Build());

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                zirconEmbed.AddField("Warning:", "You must be connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                await msg.ModifyAsync(msg => msg.Embed = new ZirconEmbedBuilder(ZirconEmbedType.Warning)
                .AddField("Not found", "Please provide search terms")
                .Build());
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await JoinAsync(true);
            }
            else 
            {
                if (voiceState.VoiceChannel != _lavaNode.GetPlayer(Context.Guild).VoiceChannel)
                {
                    zirconEmbed.AddField("Warning:", "You need to be in the same voice channel as me!");
                    await ReplyAsync(embed: zirconEmbed.Build());
                    return;
                }
            }

            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.Status == Victoria.Responses.Search.SearchStatus.LoadFailed ||
                searchResponse.Status == Victoria.Responses.Search.SearchStatus.NoMatches)
            {
                await msg.ModifyAsync(msg => msg.Embed = new ZirconEmbedBuilder(ZirconEmbedType.Warning)
                .AddField("Not found", $"I wasn't able to find anything for `{query}`.")
                .Build());
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
                    await msg.ModifyAsync(msg => msg.Embed = new ZirconEmbedBuilder()
                    .AddField($"Enqueued playlist:", $"Enqueued {searchResponse.Tracks.Count} tracks.")
                    .Build());
                }
                else
                {
                    var track = searchResponse.Tracks.ToList()[0];
                    player.Queue.Enqueue(track);

                    await msg.ModifyAsync(msg => msg.Embed = new ZirconEmbedBuilder()
                    .AddField("Added to queue:", $"[{track.Title}]({track.Url})")
                    .Build());
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
                            await _audioService.CancelDisconnect(player);
                            await player.PlayAsync(track);

                            await msg.ModifyAsync(msg => msg.Embed = new ZirconEmbedBuilder()
                            .AddField("Now Playing:", $"[{track.Title}]({track.Url})")
                            .Build());
                        }
                        else
                        {
                            player.Queue.Enqueue(searchResponse.Tracks.ToList()[i]);
                        }
                    }
                    await msg.ModifyAsync(msg => msg.Embed = new ZirconEmbedBuilder()
                    .AddField($"Enqueued playlist:", $"Enqueued {searchResponse.Tracks.Count} tracks.")
                    .Build());
                }
                else
                {
                    await _audioService.CancelDisconnect(player);
                    await player.PlayAsync(track);
                    await msg.ModifyAsync(msg => msg.Embed = new ZirconEmbedBuilder()
                    .AddField("Now Playing:", $"[{track.Title}]({track.Url})")
                    .Build());
                }
            }
        }

        [Command("join")]
        public async Task JoinAsync(bool dontDisconnect = false)
        {
            var zirconEmbed = new ZirconEmbedBuilder(ZirconEmbedType.Warning);

            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                zirconEmbed.AddField("Warning:", "I am already connected to a channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                zirconEmbed.AddField("Warning:", "You must be connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                if (!dontDisconnect)
                {
                    await _audioService.InitiateDisconnectAsync(_lavaNode.GetPlayer(Context.Guild), TimeSpan.FromSeconds(120));
                }
            }
            catch (Exception exception)
            {
                zirconEmbed.ChangeType(ZirconEmbedType.Error);
                zirconEmbed.AddField("Error:", exception.Message);
                await ReplyAsync(embed: zirconEmbed.Build());
            }
        }

        [Command("skip")]
        public async Task SkipAsync()
        {
            var zirconEmbed = new ZirconEmbedBuilder(ZirconEmbedType.Warning);

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                zirconEmbed.AddField("Warning:", "You must be connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                zirconEmbed.AddField("Warning:", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                zirconEmbed.AddField("Warning:", "You need to be in the same voice channel as me!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (player.Queue.Count == 0)
            {
                zirconEmbed.AddField("Warning:", "There are no more song in the queue!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }


            await player.SkipAsync();
            zirconEmbed.ChangeType(ZirconEmbedType.Info);
            zirconEmbed.AddField("Skipped:", $"Now playing: [{player.Track.Title}]({player.Track.Url})");
            await ReplyAsync(embed: zirconEmbed.Build());
        }

        [Command("pause")]
        public async Task PauseAsync()
        {
            var zirconEmbed = new ZirconEmbedBuilder(ZirconEmbedType.Warning);

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                zirconEmbed.AddField("Warning:", "You must be connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                zirconEmbed.AddField("Warning:", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                zirconEmbed.AddField("Warning:", "You need to be in the same voice channel as me!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
            {
                zirconEmbed.AddField("Warning:", "The music is already paused!");
                zirconEmbed.AddField("Paused:", $"[{player.Track.Title}]({player.Track.Url})");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }


            await player.PauseAsync();
            zirconEmbed.ChangeType(ZirconEmbedType.Info);
            zirconEmbed.AddField("Paused:", $"Paused: [{player.Track.Title}]({player.Track.Url})");
            await ReplyAsync(embed: zirconEmbed.Build());
        }

        [Command("resume")]
        public async Task ResumeAsync()
        {
            var zirconEmbed = new ZirconEmbedBuilder(ZirconEmbedType.Warning);

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                zirconEmbed.AddField("Warning:", "You must be connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                zirconEmbed.AddField("Warning:", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                zirconEmbed.AddField("Warning:", "You need to be in the same voice channel as me!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (player.PlayerState == PlayerState.Playing)
            {
                zirconEmbed.AddField("Warning:", "The music is already playing!");
                zirconEmbed.AddField("Playing:", $"[{player.Track.Title}]({player.Track.Url})");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }


            await player.ResumeAsync();
            zirconEmbed.ChangeType(ZirconEmbedType.Info);
            zirconEmbed.AddField("Resumed:", $"[{player.Track.Title}]({player.Track.Url})");
            await ReplyAsync(embed: zirconEmbed.Build());
        }

        [Command("queue")]
        public async Task QueueAsync()
        {

            var zirconEmbed = new ZirconEmbedBuilder(ZirconEmbedType.Warning);

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                zirconEmbed.AddField("Warning:", "You must be connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                zirconEmbed.AddField("Warning:", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                zirconEmbed.AddField("Warning:", "You need to be in the same voice channel as me!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (player.Queue.Count == 0)
            {
                zirconEmbed.AddField("Warning:", "There are no more song in the queue!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            int i = 0;
            string queueString = "";

            foreach (var item in player.Queue)
            {
                i++;
                queueString += $"\n{i} - [{item.Title}]({item.Url})";
            }

            await player.ResumeAsync();
            zirconEmbed.ChangeType(ZirconEmbedType.Info);
            zirconEmbed.AddField("Queue", $"Current playing music:\n[{player.Track.Title}]({player.Track.Url})");
            zirconEmbed.AddField("Current music in queue", queueString);
            await ReplyAsync(embed: zirconEmbed.Build());
        }

        [Command("clear")]
        public async Task ClearAsync()
        {
            var zirconEmbed = new ZirconEmbedBuilder(ZirconEmbedType.Warning);

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                zirconEmbed.AddField("Warning:", "You must be connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                zirconEmbed.AddField("Warning:", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                zirconEmbed.AddField("Warning:", "You need to be in the same voice channel as me!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (player.Queue.Count == 0)
            {
                zirconEmbed.AddField("Warning:", "There are no more song in the queue!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            player.Queue.Clear();
            zirconEmbed.ChangeType(ZirconEmbedType.Info);
            zirconEmbed.AddField("Cleared:", "Cleared the current queue!");
            await ReplyAsync(embed: zirconEmbed.Build());
        }

        [Command("stop")]
        public async Task StopAsync()
        {
            var zirconEmbed = new ZirconEmbedBuilder(ZirconEmbedType.Warning);

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                zirconEmbed.AddField("Warning:", "You must be connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                zirconEmbed.AddField("Warning:", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                zirconEmbed.AddField("Warning:", "You need to be in the same voice channel as me!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (player.Track == null)
            {
                zirconEmbed.AddField("Warning:", "There is no song playing!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            zirconEmbed.AddField("Stopped:", $"[{player.Track.Title}]({player.Track.Url})!");

            await player.StopAsync();
            await ReplyAsync(embed: zirconEmbed.Build());
        }

        [Command("leave")]
        public async Task LeaveAsync()
        {
            var zirconEmbed = new ZirconEmbedBuilder(ZirconEmbedType.Warning);

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                zirconEmbed.AddField("Warning:", "You must be connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                zirconEmbed.AddField("Warning:", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                zirconEmbed.AddField("Warning:", "You need to be in the same voice channel as me!");
                await ReplyAsync(embed: zirconEmbed.Build());
                return;
            }

            try
            {
                zirconEmbed.ChangeType(ZirconEmbedType.Info);
                zirconEmbed.AddField("Left the voice channel!", "You can reinvite the bot via \"!join\" or \"!play (YouTube song name or link)\"");
                await ReplyAsync(embed: zirconEmbed.Build());
                await _audioService.InitiateDisconnectAsync(_lavaNode.GetPlayer(Context.Guild), TimeSpan.FromSeconds(0));
            }
            catch (Exception exception)
            {
                zirconEmbed.ChangeType(ZirconEmbedType.Error);
                zirconEmbed.AddField("Error:", exception.Message);
                await ReplyAsync(embed: zirconEmbed.Build());
            }
        }
    }
}
