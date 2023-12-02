using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioPlaying;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Pause;
public sealed record PauseCommand(IInteractionContext Context) : IAudioIsInVoiceChannelPipeline, IAudioPlayingPipeline, ICommand;