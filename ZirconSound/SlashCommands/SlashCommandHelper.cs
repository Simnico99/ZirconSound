using System.Collections.Generic;
using System.Reflection;

namespace ZirconSound.SlashCommands
{
    internal class SlashCommandHelper
    {
        public static IEnumerable<SlashCommandGroup> GetSlashCommands(Assembly assembly)
        {
            var slashCommandGroups = new List<SlashCommandGroup>();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(SlashModuleBase<SlashCommandContext>)))
                {
                    foreach (var method in type.GetMethods())
                    {
                        if (method.GetCustomAttributes(typeof(SlashCommand), false).Length > 0)
                        {
                            var slashCommandGroup = new SlashCommandGroup()
                            {
                                Command = method.GetCustomAttribute<SlashCommand>(),
                                Method = method,
                                CommandModule = type
                            };

                            slashCommandGroups.Add(slashCommandGroup);
                        }
                    }
                }
            }

            return slashCommandGroups;
        }
    }
}
