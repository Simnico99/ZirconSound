using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioIsNotPlaying;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Resume;
public sealed record ResumeCommand(IInteractionContext Context) : IAudioIsInVoiceChannelPipeline, IAudioIsNotPlayingPipeline, ICommand;