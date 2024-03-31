﻿using Lavalink4NET.Clients;
using Lavalink4NET.DiscordNet;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioIsNotPlaying;
using ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioPlaying;
using ZirconSound.Application.Handlers;
using ZirconSound.Application.Interfaces;
using ZirconSound.Infrastructure.BackgroundServices;

namespace ZirconSound.Console.Startup;
public static partial class IServiceCollectionExtension
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddMediator();

        services.AddSingleton<ILavalinkRunnerService, LavalinkRunnerService>();
        services.AddHostedService<LavalinkRunnerService>(x => (LavalinkRunnerService)x.GetRequiredService<ILavalinkRunnerService>());
        services.AddHostedService<BotStatusService>();

        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(AudioAutoJoinBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(AudioPlayingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(AudioIsNotPlayingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(AudioIsInVoiceChannelBehavior<,>));
        services.AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();
        services.AddHostedService<InteractionHandler>();

        return services;
    }
}
