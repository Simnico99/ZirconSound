﻿using Discord;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.SkipCommand;
public sealed record QueueCommand(IInteractionContext Context, int? Page) : IAudioIsInVoiceChannelPipeline, ICommand;