using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using Discord;
using System.Text;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Queue;

public sealed class QueueHandler : ICommandHandler<QueueCommand>
{
    private readonly IAudioService _audioService;

    public QueueHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async ValueTask<Unit> Handle(QueueCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = await _audioService.GetPlayerAndSetContextAsync(command.Context.Guild.Id, command.Context);

        var page = command.Page;
        page ??= 0;

        if (page == 0)
        {
            page++;
        }

        var currentTrack = player!.CurrentTrack;
        var tracks = player.Queue;
        page--;

        if (tracks?.Count >= 1)
        {
            var stringBuilder = new StringBuilder();
            var estimatedTime = TimeSpan.FromSeconds(0);
            var tracksChunk = tracks.ChunkBy(5).ToList();

            for (var i = (int)page * 5; i < tracks.Count && i < (page * 5) + 5; i++)
            {
                var track = player.Queue.ElementAt(i).Track;
                stringBuilder.AppendLine($"{i}- [{track?.Title}]({track?.Uri})");
            }

            estimatedTime = tracks.Aggregate(estimatedTime, (current, trackItem) => current + trackItem.Track!.Duration);

            embed.AddField("Queue", stringBuilder.ToString());

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

            if (tracksChunk.Count > 1)
            {
                embed.WithFooter($"{page + 1},{tracksChunk.Count}");
                embed.AddField(new EmbedFieldBuilder().WithName("Pages").WithValue($"{page + 1} of {tracksChunk.Count}").WithIsInline(true));
            }

            var button = new ComponentBuilder()
                .WithButton("First", "queue-button-first", ButtonStyle.Secondary, disabled: firstDisabled)
                .WithButton("Back", $"queue-button-back", ButtonStyle.Secondary, disabled: firstDisabled)
                .WithButton("Clear", "clear-button")
                .WithButton("Next", $"queue-button-next", ButtonStyle.Secondary, disabled: lastDisabled)
                .WithButton("Last", $"queue-button-last", ButtonStyle.Secondary, disabled: lastDisabled);

            embed.AddField(new EmbedFieldBuilder().WithName("Estimated play time").WithValue(estimatedTime).WithIsInline(true));

            await command.Context.ReplyToLastCommandAsync(embed: embed.Build(), component: button.Build());
            return Unit.Value;
        }

        if (player?.Queue.Count == 0)
        {
            embed.AddField("Empty:", "The queue is empty.");
        }

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build());

        return Unit.Value;
    }
}