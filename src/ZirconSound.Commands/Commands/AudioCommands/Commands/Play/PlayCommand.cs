using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Play;
public sealed record PlayCommand(IInteractionContext Context, string Id) : IAudioAutoJoinPipeline, ICommand;