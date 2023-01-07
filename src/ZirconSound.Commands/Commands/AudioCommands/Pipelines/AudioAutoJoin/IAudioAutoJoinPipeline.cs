using Discord;
using Mediator;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
public interface IAudioAutoJoinPipeline
{
    IInteractionContext Context { get; init; }
}
