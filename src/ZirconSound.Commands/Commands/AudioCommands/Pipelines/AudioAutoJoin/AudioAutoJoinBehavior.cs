using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;
using ZirconSound.Core.Enums;
using Discord;
using Lavalink4NET.Players;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players.Queued;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
public sealed class AudioAutoJoinBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse> where TMessage : Mediator.IMessage
{
    private readonly IAudioService _audioService;
    private readonly ILogger _logger;


    public AudioAutoJoinBehavior(IAudioService audioService, ILogger<AudioAutoJoinBehavior<TMessage, TResponse>> logger)
    {
        _audioService = audioService;
        _logger = logger;
    }
    public async ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (message is IAudioAutoJoinPipeline audioMessage)
        {
            var player = await _audioService.Players.GetPlayerAsync<LoopingQueuedLavalinkPlayer>(audioMessage.Context.Guild.Id);

            var embed = EmbedHelpers.CreateGenericEmbedBuilder(audioMessage.Context);

            if (audioMessage.Context.User is IVoiceState voiceState)
            {
                if (voiceState.VoiceChannel is null)
                {
                    embed.AddField("Can't join channel:", "You are not in an voice Channel!");
                    await audioMessage.Context.ReplyToLastCommandAsync(embed: embed.Build(GenericEmbedType.Warning));
                    return default!;
                }

                if (player?.VoiceChannelId is not null && player?.VoiceChannelId != voiceState.VoiceChannel.Id)
                {
                    embed.AddField("Can't join channel:", "The bot is already in another voice channel!");
                    await audioMessage.Context.ReplyToLastCommandAsync(embed: embed.Build(GenericEmbedType.Warning));
                    return default!;
                }

            }

            if (player is null)
            {
                await JoinAsync(audioMessage);
            }
        }

        return await next(message, cancellationToken);
    }

    private async Task JoinAsync(IAudioAutoJoinPipeline message)
    {
        var voiceState = message.Context.User as IVoiceState;

        var voiceChannel = voiceState?.VoiceChannel;
        if (voiceChannel != null)
        {
            var playerOptions = new QueuedLavalinkPlayerOptions
            {
                SelfDeaf = true,
                InitialVolume = 0.25F,
            };

            var playerFactory = PlayerFactory.Create<LoopingQueuedLavalinkPlayer, QueuedLavalinkPlayerOptions>(static properties => new LoopingQueuedLavalinkPlayer(properties));
            var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: PlayerChannelBehavior.Join);
            await _audioService.Players.RetrieveAsync(message.Context, playerFactory, playerOptions, retrieveOptions);
        }
    }
}
