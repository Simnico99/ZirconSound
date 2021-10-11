using System;
using Lavalink4NET;
using Lavalink4NET.Logging;

namespace ZirconSound.Hosting.Lavalink
{
    public class HostingLavalinkNode : LavalinkNode
    {
        private readonly IDiscordClientWrapper _discordClient;

        private bool _disposed;

        public HostingLavalinkNode(LavalinkNodeOptions options, IDiscordClientWrapper client, IDiscordClientWrapper discordClient, ILogger logger = null, ILavalinkCache cache = null) : base(options, client, logger, cache)
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
            GC.SuppressFinalize(this);
        }
    }
}
