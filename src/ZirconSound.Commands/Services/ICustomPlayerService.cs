using Lavalink4NET.Player;

namespace ZirconSound.Application.Services;
public interface ICustomPlayerService
{
    void CancelAloneDisconnect(LavalinkPlayer? player);
    void CancelIdleDisconnect(LavalinkPlayer? player);
    void StartDisconnectBotIsAloneTimer(LavalinkPlayer? player, TimeSpan timeSpan);
    void StartIdleDisconnectTimer(LavalinkPlayer? player, TimeSpan timeSpan);
}