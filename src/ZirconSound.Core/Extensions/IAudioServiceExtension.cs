using Discord;
using Lavalink4NET;
using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZirconSound.Core.SoundPlayers;

namespace ZirconSound.Core.Extensions;
public static class IAudioServiceExtension
{
    public static GenericQueuedLavalinkPlayer? GetPlayerAndSetContext(this IAudioService audioService, ulong id, IInteractionContext interactionContext)
    {
        var player = audioService.GetPlayer<GenericQueuedLavalinkPlayer>(id);

        if (player is not null)
        {
            player.Context = interactionContext;
        }

        return player;
    }
}
