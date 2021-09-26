using Discord;
using System;

namespace ZirconSound.SlashCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SlashCommand : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SlashCommandOptionBuilder Options { get; set; }
        public ApplicationCommandOptionType CommandOptionType { get; set; }

        public SlashCommand(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public SlashCommand(string name, string description, string optionName, ApplicationCommandOptionType optionType, string optionDesc, bool required)
        {
            Name = name;
            Description = description;
            Options = new SlashCommandOptionBuilder().WithName(optionName).WithType(optionType).WithDescription(optionDesc).WithRequired(required);
            CommandOptionType = optionType;
        }
    }
}
