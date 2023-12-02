using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioPlaying;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Stop;
public sealed record StopCommand(IInteractionContext Context) : IAudioIsInVoiceChannelPipeline, IAudioPlayingPipeline, ICommand;