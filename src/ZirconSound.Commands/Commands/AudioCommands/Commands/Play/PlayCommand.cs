﻿using Discord;
using Mediator;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
using ZirconSound.Core.Enums;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Play;
public sealed record PlayCommand(IInteractionContext Context, string Id, SearchProvider SearchProvider = SearchProvider.None) : IAudioAutoJoinPipeline, ICommand;