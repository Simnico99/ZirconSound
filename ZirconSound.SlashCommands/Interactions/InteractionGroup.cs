using System;
using System.Reflection;

namespace ZirconSound.ApplicationCommands.Interactions
{
    internal class InteractionGroup : IInteractionGroup
    {
        public MethodInfo Method { get; set; }
        public Type Module { get; set; }
    }
}
