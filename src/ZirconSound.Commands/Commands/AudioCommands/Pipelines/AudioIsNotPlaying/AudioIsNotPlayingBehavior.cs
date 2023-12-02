using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.SoundPlayers;
using ZirconSound.Core.Enums;
using Lavalink4NET.Players;

namespace ZirconSound.Application.Commands.AudioCommands.Pipelines.AudioIsNotPlaying;
public sealed class AudioIsNotPlayingBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse> where TMessage : Mediator.IMessage
{
    private readonly IAudioService _audioService;

    public AudioIsNotPlayingBehavior(IAudioService audioService)
    {
        _audioService = audioService;
    }
    public async ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (message is IAudioIsNotPlayingPipeline audioMessage)
        {
            if (_audioService.Players.TryGetPlayer<LoopingQueuedLavalinkPlayer>(audioMessage.Context.Guild.Id, out var player))
            {
                var embed = EmbedHelpers.CreateGenericEmbedBuilder(audioMessage.Context);

                if (player is not null && player.State is PlayerState.Playing)
                {
                    embed.AddField("Track is already playing", "A track is already playing.");
                    await audioMessage.Context.ReplyToLastCommandAsync(embed: embed.Build(GenericEmbedType.Warning));
                    return default!;
                }
            }

        }

        return await next(message, cancellationToken);
    }

}
