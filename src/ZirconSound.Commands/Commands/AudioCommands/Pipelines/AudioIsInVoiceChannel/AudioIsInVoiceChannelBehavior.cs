using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;
using ZirconSound.Core.Enums;
using Discord;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioAutoJoin;
public sealed class AudioIsInVoiceChannelBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse> where TMessage : Mediator.IMessage
{
    private readonly IAudioService _audioService;

    public AudioIsInVoiceChannelBehavior(IAudioService audioService)
    {
        _audioService = audioService;
    }
    public async ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (message is IAudioIsInVoiceChannelPipeline audioMessage)
        {
            var player = _audioService.GetPlayer<GenericQueuedLavalinkPlayer>(audioMessage.Context.Guild.Id);

            var embed = EmbedHelpers.CreateGenericEmbedBuilder(audioMessage.Context);

            if (audioMessage.Context.User is IVoiceState voiceState)
            {
                if (player?.VoiceChannelId is null)
                {
                    embed.AddField("Is not in voice:", "The bot is not in a voice channel!");
                    await audioMessage.Context.ReplyToLastCommandAsync(embed: embed.Build(GenericEmbedType.Warning));
                    return default!;
                }

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
        }

        return await next(message, cancellationToken);
    }
}
