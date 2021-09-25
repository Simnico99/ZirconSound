using System;
using System.Reflection;

namespace ZirconSound.SlashCommands
{
    public class SlashCommandGroup
    {
        public SlashCommand Command { get; set; }
        public MethodInfo Method { get; set; }
        public Type CommandModule { get; set; }
    }
}
