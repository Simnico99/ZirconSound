using Discord;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.PlayCommand;
public sealed record PlayCommand(IInteractionContext Context, string Id) : IAudioAutoJoinPipeline, ICommand;