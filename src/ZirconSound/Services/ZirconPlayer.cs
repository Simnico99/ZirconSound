using ZirconSound.Helpers;

namespace ZirconSound.Services;

public class ZirconPlayer : QueuedLavalinkPlayer
{
    public bool IsCustomLooping { get; set; }
    public bool LoopSkip { get; set; } = false;
    public LavalinkTrack? CurrentLoopingTrack { get; set; }
    public LockHelper Locker { get; set; } = new LockHelper();
    public int ErrorRetry { get; set; }
    public LavalinkTrack LastErrorTrack { get; set; }
    public IInteractionContext Context { get; private set; }

    public void SetInteraction(IInteractionContext context) => Context = context;
}

