using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Volume;
public sealed record VolumeCommand(IInteractionContext Context, float Volume, float? VolumeButton = null) : IAudioIsInVoiceChannelPipeline, ICommand;