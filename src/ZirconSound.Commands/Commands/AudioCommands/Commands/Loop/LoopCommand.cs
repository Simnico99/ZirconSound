using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
using ZirconSound.Core.Enums;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Loop;
public sealed record LoopCommand(IInteractionContext Context, LoopType LoopType) : IAudioIsInVoiceChannelPipeline, ICommand;