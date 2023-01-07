using Discord;
using Mediator;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
public interface IAudioIsInVoiceChannelPipeline
{
    IInteractionContext Context { get; init; }
}
