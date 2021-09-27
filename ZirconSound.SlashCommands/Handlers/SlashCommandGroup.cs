using System;
using System.Reflection;

namespace ZirconSound.SlashCommands.Handlers
{
    public class SlashCommandGroup
    {
        public SlashCommand Command { get; init; }
        public MethodInfo Method { get; init; }
        public Type CommandModule { get; init; }
    }
}