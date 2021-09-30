using System;
using System.Reflection;

namespace ZirconSound.ApplicationCommands.Interactions
{
    public interface IInteractionGroup
    {
        MethodInfo Method { get; init; }
        Type CommandModule { get; init; }
    }
}