namespace ZirconSound.ApplicationCommands.Interactions;

internal class InteractionsGroup : IInteractionGroup
{
    public MethodInfo Method { get; set; }
    public Type Module { get; set; }
}
