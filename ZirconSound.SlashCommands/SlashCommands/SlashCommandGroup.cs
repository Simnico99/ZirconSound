using System;
using System.Reflection;
using ZirconSound.ApplicationCommands.Interactions;

namespace ZirconSound.ApplicationCommands.SlashCommands
{
    public class SlashCommandGroup : IInteractionGroup
    {
        public SlashCommand Command { get; init; }
        public MethodInfo Method { get; init; }
        public Type CommandModule { get; init; }
    }
}