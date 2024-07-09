using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using Discord;
using Lavalink4NET.Rest.Entities.Tracks;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Play;

public sealed class PlayHandler : ICommandHandler<PlayCommand>
{
    private readonly IAudioService _audioService;

    public PlayHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async ValueTask<Unit> Handle(PlayCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = await _audioService.GetPlayerAndSetContextAsync(command.Context.Guild.Id, command.Context);
        var trackLoadResult = await _audioService.Tracks.LoadTrackFromProvider(command.Id, command.SearchProvider, cancellationToken);

        if (trackLoadResult.IsFailed || player is null)
        {
            if (command.Id.Contains("youtube.com/watch"))
            {
                embed.AddField("Warning:", "Unable to find the specified track!\n(This video might be age restricted. ZirconSound cannot play age restricted video on youtube anymore try '/playwithprovider' and use another provider than youtube.)");
            }
            else
            {
                embed.AddField("Warning:", "Unable to find the specified track!");
            }

            await command.Context.ReplyToLastCommandAsync(embed: embed.Build(GenericEmbedType.Warning), ephemeral: false);
            return Unit.Value;
        }

        var playerTrackWasNull = player?.CurrentTrack is null;

        var currentTrack = trackLoadResult.Tracks.FirstOrDefault()!;
        var tracks = trackLoadResult.Tracks.Skip(1);

        if (trackLoadResult.IsPlaylist && trackLoadResult.Tracks.Length <= 1)
        {
            trackLoadResult = TrackLoadResult.CreateTrack(trackLoadResult.Track);
        }

        if (playerTrackWasNull)
        {
            embed.AddField("Playing:", $"[{currentTrack.Title}]({currentTrack.Uri})");
            embed.EmbedSong(currentTrack);

            await player!.PlayAsync(currentTrack, cancellationToken: cancellationToken);
        }

        if (!trackLoadResult.IsPlaylist && !playerTrackWasNull)
        {
            if (player?.CurrentLoopingPlaylist is not null)
            {
                player.CurrentLoopingPlaylist.Add(currentTrack);
            }

            embed.AddField("Queued:", $"[{currentTrack.Title}]({currentTrack.Uri})");

            var timeLeft = TimeSpan.FromSeconds(0);
            timeLeft = player!.Queue!.Aggregate(timeLeft, (current, trackQueue) => current + trackQueue.Track!.Duration);
            timeLeft += player.CurrentTrack!.Duration - player.Position!.Value.Position.StripMilliseconds();

            var estimatedTime = new EmbedFieldBuilder().WithName("Estimated time until track").WithValue(timeLeft.StripMilliseconds()).WithIsInline(true);
            embed.AddField(estimatedTime);
            embed.AddField("Position in queue", player.Queue!.Count + 1);
            embed.EmbedSong(currentTrack);

            await player.PlayAsync(currentTrack, true, cancellationToken: cancellationToken);
        }

        if (trackLoadResult.IsPlaylist)
        {
            var estimatedTime = new EmbedFieldBuilder().WithName("Queued:").WithValue($"{tracks.Count()} tracks!").WithIsInline(true);
            embed.AddField(estimatedTime);
            var timeLeft = TimeSpan.FromSeconds(0);

            foreach (var track in tracks)
            {
                if (player?.CurrentLoopingPlaylist is not null)
                {
                    player.CurrentLoopingPlaylist.Add(track);
                }

                timeLeft += track.Duration;
                await player!.PlayAsync(track, true, cancellationToken: cancellationToken);
            }

            embed.AddField("Estimated play time:", $"{timeLeft}");
        }

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build());

        return Unit.Value;
    }
}
