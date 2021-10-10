using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Payloads.Events;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Microsoft.Extensions.Logging;
using ZirconSound.ApplicationCommands.Interactions;
using ZirconSound.Embeds;

namespace ZirconSound.Services
{
    public class ZirconPlayer : QueuedLavalinkPlayer
    {
        public IInteractionContext Context { get; private set; }

        public void SetInteraction(IInteractionContext context)
        {
            Context = context;
        }
    }
}

