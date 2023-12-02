using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
public interface IAudioIsInVoiceChannelPipeline
{
    IInteractionContext Context { get; init; }
}
