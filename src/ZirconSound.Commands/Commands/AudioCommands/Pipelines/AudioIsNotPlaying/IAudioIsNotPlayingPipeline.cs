using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioIsNotPlaying;
public interface IAudioIsNotPlayingPipeline
{
    IInteractionContext Context { get; init; }
}
