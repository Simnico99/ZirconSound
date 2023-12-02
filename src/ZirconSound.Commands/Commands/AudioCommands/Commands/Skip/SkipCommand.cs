using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Skip;
public sealed record SkipCommand(IInteractionContext Context) : IAudioIsInVoiceChannelPipeline, ICommand;