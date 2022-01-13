namespace ZirconSound.Services;

public class ZirconPlayer : QueuedLavalinkPlayer
{
    public int ErrorRetry { get; set; }
    public LavalinkTrack LastErrorTrack { get; set; }

    public IInteractionContext Context { get; private set; }

    public void SetInteraction(IInteractionContext context) => Context = context;
}

