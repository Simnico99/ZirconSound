namespace ZirconSound.Services;

public interface IPlayerService
{
    Task BotIsAloneAsync(LavalinkPlayer player, TimeSpan timeSpan);
    Task CancelAloneDisconnectAsync(LavalinkPlayer player);
    Task CancelDisconnectAsync(LavalinkPlayer player);
    Task InitiateDisconnectAsync(LavalinkPlayer player, TimeSpan timeSpan);
}
