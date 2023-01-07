using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;
using ZirconSound.Core.Enums;
using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
public sealed class AudioAutoJoinBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse> where TMessage : Mediator.IMessage
{
    private readonly IAudioService _audioService;

    public AudioAutoJoinBehavior(IAudioService audioService)
    {
        _audioService = audioService;
    }
    public async ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (message is IAudioAutoJoinPipeline audioMessage)
        {
            var player = _audioService.GetPlayer<GenericQueuedLavalinkPlayer>(audioMessage.Context.Guild.Id);

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
            await _audioService.JoinAsync<GenericQueuedLavalinkPlayer>(message.Context.Guild.Id, voiceChannel.Id, true);
        }
    }
}
