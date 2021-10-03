using System;
using System.Reflection;
using ZirconSound.ApplicationCommands.Interactions;

namespace ZirconSound.ApplicationCommands.SlashCommands
{
    public class SlashCommandGroup : SlashCommandGroup<SlashCommandAttribute>, IInteractionGroup
    { }

    public class SlashCommandGroup<T> : IInteractionGroup<T> where T : SlashCommandAttribute
    {
        public T Interaction { get; set; }
        public MethodInfo Method { get; set; }
        public Type Module { get; set; }
    }
}