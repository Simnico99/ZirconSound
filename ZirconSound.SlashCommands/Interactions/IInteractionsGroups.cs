using System;
using System.Reflection;

namespace ZirconSound.ApplicationCommands.Interactions
{
    public interface IInteractionGroup<T>
    {
        T Interaction { get; set; }
        MethodInfo Method { get; set; }
        Type Module { get; set; }
    }
}