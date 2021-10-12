

namespace ZirconSound.ApplicationCommands.Interactions;

public interface IInteractionGroup
{
    MethodInfo Method { get; set; }
    Type Module { get; set; }
}

public interface IInteractionGroup<T>
{
    T Interaction { get; set; }
    MethodInfo Method { get; set; }
    Type Module { get; set; }
}
