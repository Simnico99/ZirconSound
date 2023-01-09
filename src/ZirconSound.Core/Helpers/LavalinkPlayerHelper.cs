using Lavalink4NET.Player;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZirconSound.Core.SoundPlayers;

namespace ZirconSound.Core.Helpers;
public static class LavalinkPlayerHelper
{
    private static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _aloneDisconnectTokens = new();
    private static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens = new();

    public static void StartDisconnectBotIsAloneTimer(LavalinkPlayer? player, TimeSpan timeSpan)
    {
        Log.Debug("Disconnecting alone timer started waiting: {time}", timeSpan);

        if (player is null)
        {
            return;
        }

        var cancellationSourceGetResult = _aloneDisconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value);
        value?.Cancel();
        value = null;

        if (!cancellationSourceGetResult || value is null)
        {
            value = new CancellationTokenSource();
            _aloneDisconnectTokens.TryAdd((ulong)player.VoiceChannelId!, value);
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(timeSpan, value.Token);
            }
            catch (TaskCanceledException ex)
            {
                Log.Debug(ex, "Canceled Alone disconnect");
            }
            finally
            {
                if (!value.Token.IsCancellationRequested)
                {
                    await player.DisconnectAsync();
                    Log.Debug("Alone Disconnected");
                }
            }
        });
    }

    public static void StartIdleDisconnectTimer(LavalinkPlayer? player, TimeSpan timeSpan)
    {
        Log.Debug("Disconnecting idle timer started waiting: {time}", timeSpan);

        if (player is null)
        {
            return;
        }

        var cancellationSourceGetResult = _disconnectTokens.TryGetValue(player.VoiceChannelId ?? 0, out var value);
        value?.Cancel();
        value = null;

        if (!cancellationSourceGetResult || value is null)
        {
            value = new CancellationTokenSource();
            _disconnectTokens.TryAdd((ulong)player.VoiceChannelId!, value);
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(timeSpan, value.Token);
            }
            catch (TaskCanceledException ex)
            {
                Log.Debug(ex, "Canceled Idle disconnect");
            }
            finally
            {
                if (!value.Token.IsCancellationRequested)
                {
                    await player.DisconnectAsync();
                    Log.Debug("Idle Disconnected");
                }
            }
        });
    }

    public static void CancelAloneDisconnect(LavalinkPlayer? player)
    {
        Log.Debug("Canceling alone disconnect");
        if (player is GenericQueuedLavalinkPlayer genericPlayer)
        {
            _ = _aloneDisconnectTokens.TryGetValue(genericPlayer.VoiceChannelId ?? 0, out var value);
            value?.Cancel();
        }
    }

    public static void CancelIdleDisconnect(LavalinkPlayer? player)
    {
        Log.Debug("Canceling idle disconnect");
        if (player is GenericQueuedLavalinkPlayer zirconplayer)
        {
            _ = _disconnectTokens.TryGetValue(zirconplayer.VoiceChannelId ?? 0, out var value);
            value?.Cancel();
        }
    }
}
