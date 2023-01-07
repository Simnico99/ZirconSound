using Discord.Interactions;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Commands.PlayCommand;
using ZirconSound.Core.Enums;
using ZirconSound.Application.Commands.AudioCommands.Commands.SkipCommand;
using ZirconSound.Core.Extensions;

namespace ZirconSound.Application.Commands.AudioCommands;
public sealed class AudioCommands : InteractionModuleBase<IInteractionContext>
{
    private readonly IMediator _mediator;

    public AudioCommands(IMediator mediator)
    {
        _mediator = mediator;
    }

    [SlashCommand("play", "Play a track.")]
    public async Task PlayAsync(string id) => await _mediator.Send(new PlayCommand(Context, id));

    [SlashCommand("loop", "Loop either a track or the playlist.")]
    public async Task LoopAsync(LoopType loopType) => await _mediator.Send(new LoopCommand(Context, loopType));

    [ComponentInteraction("loop-button-stop")]
    public async Task LoopStopAsync() => await _mediator.Send(new LoopCommand(Context, LoopType.Cancel));

    [ComponentInteraction("loop-button-track")]
    public async Task LoopTrackAsync() => await _mediator.Send(new LoopCommand(Context, LoopType.Track));

    [ComponentInteraction("loop-button-playlist")]
    public async Task LoopPlaylistAsync() => await _mediator.Send(new LoopCommand(Context, LoopType.Playlist));

    [SlashCommand("skip", "Skip the current track.")]
    public async Task SkipAsync() => await _mediator.Send(new SkipCommand(Context));

    [SlashCommand("queue", "Skip the current track.")]
    public async Task QueueAsync(int? page = 1) => await _mediator.Send(new QueueCommand(Context, page));

    [ComponentInteraction("queue-button-first")]
    public async Task QueueFirstAsync() => await _mediator.Send(new QueueCommand(Context, 1));

    [ComponentInteraction("queue-button-next")]
    public async Task QueueNextAsync()
    {
        var test = await Context.Interaction.GetOriginalResponseAsync();
        var footer = test.Embeds.First().Footer;
        if (footer is not null)
        {
            var footerContent = footer.Value.Text;
            var page = footerContent.Split(',')[0];

            await _mediator.Send(new QueueCommand(Context, int.Parse(page) + 1));
        }
    }

    [ComponentInteraction("queue-button-last")]
    public async Task QueueLastAsync()
    {
        var test = await Context.Interaction.GetOriginalResponseAsync();
        var footer = test.Embeds.First().Footer;
        if (footer is not null)
        {
            var footerContent = footer.Value.Text;
            var page = footerContent.Split(',')[1];

            await _mediator.Send(new QueueCommand(Context, int.Parse(page)));
        }
    }

    [ComponentInteraction("queue-button-back")]
    public async Task QueueBackAsync()
    {
        var test = await Context.Interaction.GetOriginalResponseAsync();
        var footer = test.Embeds.First().Footer;
        if (footer is not null)
        {
            var footerContent = footer.Value.Text;
            var page = footerContent.Split(',')[0];

            await _mediator.Send(new QueueCommand(Context, int.Parse(page) - 1));
        }
    }

    [SlashCommand("clear", "Clear the playlist.")]
    [ComponentInteraction("clear-button")]
    public async Task ClearAsync() => await _mediator.Send(new ClearCommand(Context));

    [SlashCommand("pause", "Pause the current track.")]
    [ComponentInteraction("track-button-pause")]
    public async Task PauseAsync() => await _mediator.Send(new PauseCommand(Context));

    [SlashCommand("resume", "Resume the current track.")]
    [ComponentInteraction("track-button-resume")]
    public async Task ResumeAsync() => await _mediator.Send(new ResumeCommand(Context));

    [SlashCommand("stop", "Stop the current track and clear the queue.")]
    public async Task StopAsync() => await _mediator.Send(new StopCommand(Context));
    [SlashCommand("leave", "Leave the current audio channel.")]
    public async Task LeaveAsync() => await _mediator.Send(new LeaveCommand(Context));
}
