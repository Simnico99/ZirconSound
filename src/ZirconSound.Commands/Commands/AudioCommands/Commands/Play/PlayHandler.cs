using Lavalink4NET;
using Lavalink4NET.Rest;
using Mediator;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;
using ZirconSound.Application.Services;
using Discord;
using Lavalink4NET.Player;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.PlayCommand;

public sealed class PlayHandler : ICommandHandler<PlayCommand>
{
    private readonly IAudioService _audioService;
    private readonly ICustomPlayerService _customPlayerService;

    public PlayHandler(IAudioService audioService, ICustomPlayerService customPlayerService)
    {
        _audioService = audioService;
        _customPlayerService = customPlayerService;
    }

    public async ValueTask<Unit> Handle(PlayCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = _audioService.GetPlayerAndSetContext(command.Context.Guild.Id, command.Context);
        var tracks = await _audioService.GetTracksAsync(command.Id, SearchMode.None, true, cancellationToken);

        if (!tracks.Any())
        {
            tracks = new List<LavalinkTrack> { (await _audioService.GetTrackAsync(command.Id, SearchMode.YouTube, true, cancellationToken))! };
        }

        if (tracks?.FirstOrDefault() is null || player is null)
        {
            embed.AddField("Warning:", "Unable to find the specified track!");
            await command.Context.ReplyToLastCommandAsync(embed: embed.Build(GenericEmbedType.Warning), ephemeral: false);
            return Unit.Value;
        }

        var playerTrackWasNull = player?.CurrentTrack is null;

        var currentTrack = tracks.First();
        tracks = tracks.Skip(1);

        if (playerTrackWasNull)
        {
            _customPlayerService.CancelIdleDisconnect(player);
            _customPlayerService.CancelAloneDisconnect(player);

            embed.AddField("Playing:", $"[{currentTrack.Title}]({currentTrack.Source})");
            embed.EmbedSong(currentTrack);

            await player!.PlayAsync(currentTrack);
        }

        if (tracks.Count() <= 1 && !playerTrackWasNull)
        {
            if (player?.CurrentLoopingPlaylist is not null)
            {
                player.CurrentLoopingPlaylist.Add(currentTrack);
            }

            embed.AddField("Queued:", $"[{currentTrack.Title}]({currentTrack.Source})");

            var timeLeft = TimeSpan.FromSeconds(0);
            timeLeft = player!.Queue.Tracks.Aggregate(timeLeft, (current, trackQueue) => current + trackQueue.Duration);
            timeLeft += player.CurrentTrack!.Duration - player.Position.Position.StripMilliseconds();

            var estimatedTime = new EmbedFieldBuilder().WithName("Estimated time until track").WithValue(timeLeft).WithIsInline(true);
            embed.AddField(estimatedTime);
            embed.AddField("Position in queue", player.Queue.Tracks.Count + 1);
            embed.EmbedSong(currentTrack);

            player.Queue.Add(currentTrack);
        }

        if (tracks.Count() > 1)
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
                player?.Queue.Add(track);
            }

            embed.AddField("Estimated play time:", $"{timeLeft}");
        }

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build());

        return Unit.Value;
    }
}
