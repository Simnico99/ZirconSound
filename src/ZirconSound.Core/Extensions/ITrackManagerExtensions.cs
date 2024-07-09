using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using ZirconSound.Core.Enums;

namespace ZirconSound.Core.Extensions;
public static class ITrackManagerExtensions
{
    public static async ValueTask<TrackLoadResult> LoadTrackFromProvider(this ITrackManager trackManager, string id, SearchProvider searchProvider = SearchProvider.None, CancellationToken cancellationToken = default)
    {
        var trackLoadResult = TrackLoadResult.CreateEmpty();
        id = id.TrimEnd('/');


        if (searchProvider is SearchProvider.None)
        {
            trackLoadResult = await trackManager.LoadTracksAsync(id, TrackSearchMode.None, cancellationToken: cancellationToken);
        }

        if (trackLoadResult.IsFailed || searchProvider is SearchProvider.YoutubeMusic)
        {
            trackLoadResult = await trackManager.LoadTracksAsync(id, TrackSearchMode.YouTubeMusic, cancellationToken: cancellationToken);
        }

        if (trackLoadResult.IsFailed || searchProvider is SearchProvider.Youtube)
        {
            trackLoadResult = await trackManager.LoadTracksAsync(id, TrackSearchMode.YouTube, cancellationToken: cancellationToken);
        }

        if (trackLoadResult.IsFailed || searchProvider is SearchProvider.Spotify)
        {
            trackLoadResult = await trackManager.LoadTracksAsync(id, TrackSearchMode.Spotify, cancellationToken: cancellationToken);
        }

        if (trackLoadResult.IsFailed || searchProvider is SearchProvider.SoundCloud)
        {
            trackLoadResult = await trackManager.LoadTracksAsync(id, TrackSearchMode.SoundCloud, cancellationToken: cancellationToken);
        }

        if (trackLoadResult.IsFailed || searchProvider is SearchProvider.Deezer)
        {
            trackLoadResult = await trackManager.LoadTracksAsync(id, TrackSearchMode.Deezer, cancellationToken: cancellationToken);
        }

        if (!id.StartsWith("https") && !id.StartsWith("http") && (trackLoadResult.IsFailed || searchProvider is SearchProvider.Twitch))
        {
            var twitchId = id;
            if (!twitchId.Contains("https://www.twitch.tv/", StringComparison.OrdinalIgnoreCase))
            {
                twitchId = $"https://www.twitch.tv/{twitchId}";
            }

            trackLoadResult = await trackManager.LoadTracksAsync(twitchId, TrackSearchMode.None, cancellationToken: cancellationToken);
        }

        return trackLoadResult;
    }
}
