using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Clear;
public sealed record ClearCommand(IInteractionContext Context) : IAudioIsInVoiceChannelPipeline, ICommand;
