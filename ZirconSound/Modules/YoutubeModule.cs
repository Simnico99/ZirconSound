using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Victoria.Responses.Search;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using ZirconSound.Common;

namespace ZirconSound.Modules
{
    public class LavaTrackPlaylist
    {
        public int SkippedVideo { get; set; }
        public int TotalVideo { get; set; }
        public List<LavaTrack> Tracks { get; set; }

        public LavaTrackPlaylist(int skippedVideo, int totalVideo, IEnumerable<LavaTrack> tracks)
        {
            SkippedVideo = skippedVideo;
            TotalVideo = totalVideo;
            Tracks = tracks.ToList();
        }
    }

    public class YoutubeModule
    {
        private static readonly YoutubeClient _ytClient = new();
        private readonly LavaNode _lavaNode;

        public YoutubeModule(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }

        public async Task<LavaTrackPlaylist> GetVideoPlaylist(string query, IUserMessage msg)
        {
            if (query.ToLowerInvariant().Contains("list="))
            {
                string split = "list=";
                string listId = query.Substring(query.IndexOf(split) + split.Length);

                var playlist = await _ytClient.Playlists.GetAsync(listId);
                if (playlist != null)
                {
                    var videosSubset = await _ytClient.Playlists
                    .GetVideosAsync(playlist.Id)
                    .CollectAsync(100);
                    if (videosSubset != null)
                    {
                        var skippedVideo = 0;
                        var playListLava = new List<LavaTrack>();
                        var listTask = new List<Task<SearchResponse>>();
                        foreach (var video in videosSubset)
                        {
                            listTask.Add(Task.Run(() => _lavaNode.SearchYouTubeAsync(video.Url.Replace($"&list={listId}", ""))));
                        }

                        var results = await Task.WhenAll(listTask);

                        foreach (var response in results)
                        {
                            if (response.Status == SearchStatus.LoadFailed ||
                                response.Status == SearchStatus.NoMatches)
                            {
                                skippedVideo++;
                            }

                            var track = response.Tracks.FirstOrDefault();
                            playListLava.Add(track);
                        }
                        return new LavaTrackPlaylist(skippedVideo, playListLava.Count(), playListLava);
                    }

                }
            }
            return null;
        }
    }
}
