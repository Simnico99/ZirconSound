using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Leave;
public sealed record LeaveCommand(IInteractionContext Context) : IAudioIsInVoiceChannelPipeline, ICommand;