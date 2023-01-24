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

    private static void RecreateTokens(ulong channelId, LavalinkPlayer player)
    {
        if (player is GenericQueuedLavalinkPlayer genericPlayer)
        {
            genericPlayer.PlayerIsAlone = false;
            genericPlayer.PlayerIsIdle = false;
        }

        _aloneDisconnectTokens.TryGetValue(channelId, out var aloneToken);
        aloneToken?.Cancel();
        aloneToken = new CancellationTokenSource();
        _aloneDisconnectTokens.TryAdd(channelId, aloneToken);

        _disconnectTokens.TryGetValue(channelId, out var idleToken);
        idleToken?.Cancel();
        idleToken = new CancellationTokenSource();
        _disconnectTokens.TryAdd(channelId, idleToken);
    }

    public static void StartDisconnectBotIsAloneTimer(LavalinkPlayer? player, TimeSpan timeSpan)
    {
        Log.Debug("Disconnecting alone timer started waiting: {time}", timeSpan);

        if (player is null)
        {
            return;
        }

        if (player is GenericQueuedLavalinkPlayer genericPlayer)
        {
            genericPlayer.PlayerIsAlone = true;
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
                    RecreateTokens((ulong)player.VoiceChannelId!, player);
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

        if (player is GenericQueuedLavalinkPlayer genericPlayer)
        {
            genericPlayer.PlayerIsIdle = true;
        }

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
                    RecreateTokens((ulong)player.VoiceChannelId!, player);
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
            genericPlayer.PlayerIsAlone = false;
            _ = _aloneDisconnectTokens.TryGetValue(genericPlayer.VoiceChannelId ?? 0, out var value);
            value?.Cancel();
        }
    }

    public static void CancelIdleDisconnect(LavalinkPlayer? player)
    {
        Log.Debug("Canceling idle disconnect");
        if (player is GenericQueuedLavalinkPlayer genericPlayer)
        {
            genericPlayer.PlayerIsAlone = false;
            _ = _disconnectTokens.TryGetValue(genericPlayer.VoiceChannelId ?? 0, out var value);
            value?.Cancel();
        }
    }
}
