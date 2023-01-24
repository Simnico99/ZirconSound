using Discord;
using Discord.Interactions;
using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZirconSound.Core.SoundPlayers;
public sealed class GenericQueuedLavalinkPlayer : QueuedLavalinkPlayer
{
    public bool PlayerIsIdle { get; set; }
    public bool PlayerIsAlone { get; set; }
    public bool PlayerGotError { get; set; }
    public bool LoopSkip { get; set; } = false;
    public bool SkippedOnPurpose { get; set; } = false;
    public LavalinkTrack? CurrentLoopingTrack { get; set; }
    public List<LavalinkTrack>? CurrentLoopingPlaylist { get; set; }
    public IInteractionContext? Context { get; set; }
}
