using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
public interface IAudioAutoJoinPipeline
{
    IInteractionContext Context { get; init; }
}
