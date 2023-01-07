using Discord;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioPlaying;
using ZirconSound.Core.Enums;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.PlayCommand;
public sealed record LoopCommand(IInteractionContext Context, LoopType LoopType) : IAudioIsInVoiceChannelPipeline, ICommand;