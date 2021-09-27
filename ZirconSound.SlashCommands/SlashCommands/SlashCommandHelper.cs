﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZirconSound.ApplicationCommands.SlashCommands
{
    internal static class SlashCommandHelper
    {
        public static IEnumerable<SlashCommandGroup> GetSlashCommands(Assembly assembly) => (from type in assembly.GetTypes()
                                                                                             where type.IsSubclassOf(typeof(SlashModuleBase<SlashCommandContext>))
                                                                                             from method in type.GetMethods()
                                                                                             where method.GetCustomAttributes(typeof(SlashCommand), false).Length > 0
                                                                                             select new SlashCommandGroup { Command = method.GetCustomAttribute<SlashCommand>(), Method = method, CommandModule = type }).ToList();
    }
}