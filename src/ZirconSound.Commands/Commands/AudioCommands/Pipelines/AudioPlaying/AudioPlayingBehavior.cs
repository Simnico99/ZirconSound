using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;
using ZirconSound.Core.Enums;
using Lavalink4NET.Players;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioPlaying;
public sealed class AudioPlayingBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse> where TMessage : Mediator.IMessage
{
    private readonly IAudioService _audioService;

    public AudioPlayingBehavior(IAudioService audioService)
    {
        _audioService = audioService;
    }
    public async ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (message is IAudioPlayingPipeline audioMessage)
        {
            var result = _audioService.Players.TryGetPlayer<LoopingQueuedLavalinkPlayer>(audioMessage.Context.Guild.Id, out var player);

            var embed = EmbedHelpers.CreateGenericEmbedBuilder(audioMessage.Context);


            if (!result)
            {
                embed.AddField("No track playing","No track is currently playing.");
                await audioMessage.Context.ReplyToLastCommandAsync(embed: embed.Build(GenericEmbedType.Warning));
                return default!;
            }

            if (player!.State is not PlayerState.Playing)
            {
                embed.AddField("No track playing", "No track is currently playing.");
                await audioMessage.Context.ReplyToLastCommandAsync(embed: embed.Build(GenericEmbedType.Warning));
                return default!;
            }
        }

        return await next(message, cancellationToken);
    }

}
