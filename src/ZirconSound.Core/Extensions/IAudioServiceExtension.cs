using Discord;
using Lavalink4NET;
using ZirconSound.Core.SoundPlayers;

namespace ZirconSound.Core.Extensions;
public static class IAudioServiceExtension
{
    public static async Task<LoopingQueuedLavalinkPlayer?> GetPlayerAndSetContextAsync(this IAudioService audioService, ulong id, IInteractionContext interactionContext)
    {
        var player = await audioService.Players.GetPlayerAsync<LoopingQueuedLavalinkPlayer>(id);

        if (player is not null)
        {
            player.Context = interactionContext;
        }

        return player;
    }
}
