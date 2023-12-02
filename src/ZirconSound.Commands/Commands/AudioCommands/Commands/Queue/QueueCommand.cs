using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Queue;
public sealed record QueueCommand(IInteractionContext Context, int? Page) : IAudioIsInVoiceChannelPipeline, ICommand;
