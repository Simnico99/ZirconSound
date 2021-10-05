#nullable enable
using System.Collections.Generic;
using Lavalink4NET;
using Lavalink4NET.Logging;
using Lavalink4NET.Player;

namespace ZirconSound.Lavalink4Net
{
    internal class HostedLavalinkNode : LavalinkNode
    {
        private readonly IDiscordClientWrapper _discordClient;

        private bool _disposed;

        public HostedLavalinkNode(LavalinkNodeOptions options, IDiscordClientWrapper client, IDiscordClientWrapper discordClient, ILogger? logger = null, ILavalinkCache? cache = null) : base(options, client, logger, cache)
        {
            _discordClient = discordClient;
        }

        public override void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _discordClient.VoiceServerUpdated -= VoiceServerUpdated;
            _discordClient.VoiceStateUpdated -= VoiceStateUpdated;
            foreach (var player in Players)
            {
                player.Value.Dispose();
            }

            Players?.Clear();
            base.Dispose();
        }


    }
}
