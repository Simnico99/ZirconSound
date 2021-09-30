using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZirconSound.ApplicationCommands.SlashCommands;

namespace ZirconSound.ApplicationCommands.Helpers
{
    internal static class ModuleHelper
    {
        public static IEnumerable<SlashCommandGroup> GetSlashModules(Assembly assembly) 
        {
            var list = new List<SlashCommandGroup>();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(SlashModuleBase<SlashCommandContext>)))
                    foreach (var method in type.GetMethods())
                    {
                        if (method.GetCustomAttributes(typeof(SlashCommand), false).Length > 0)
                        {
                            list.Add(new SlashCommandGroup { Command = method.GetCustomAttribute<SlashCommand>(), Method = method, CommandModule = type });
                        }
                    }
            }

            return list;
        }

        public static IEnumerable<SlashCommandGroup> GetInteractionModules(Assembly assembly)
        {
            var list = new List<SlashCommandGroup>();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(SlashModuleBase<SlashCommandContext>)))
                    foreach (var method in type.GetMethods())
                    {
                        if (method.GetCustomAttributes(typeof(SlashCommand), false).Length > 0)
                        {
                            list.Add(new SlashCommandGroup { Command = method.GetCustomAttribute<SlashCommand>(), Method = method, CommandModule = type });
                        }
                    }
            }

            return list;
        }
    }
}