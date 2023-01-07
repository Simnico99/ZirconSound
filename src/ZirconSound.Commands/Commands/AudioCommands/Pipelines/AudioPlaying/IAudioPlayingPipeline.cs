using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioPlaying;
public interface IAudioPlayingPipeline
{ 
    IInteractionContext Context { get; init; }
}
