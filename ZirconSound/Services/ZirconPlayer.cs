namespace ZirconSound.Services;

public class ZirconPlayer : QueuedLavalinkPlayer
{
    public IInteractionContext Context { get; private set; }

    public void SetInteraction(IInteractionContext context) => Context = context;
}

