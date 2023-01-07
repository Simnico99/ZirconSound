using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioIsNotPlaying;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioPlaying;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.SkipCommand;
public sealed record StopCommand(IInteractionContext Context) : IAudioIsInVoiceChannelPipeline, IAudioPlayingPipeline, ICommand;