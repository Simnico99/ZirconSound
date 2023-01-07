using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioIsNotPlaying;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.SkipCommand;
public sealed record ResumeCommand(IInteractionContext Context) : IAudioIsInVoiceChannelPipeline, IAudioIsNotPlayingPipeline, ICommand;