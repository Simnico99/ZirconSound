using Discord;
using System;

namespace ZirconSound.ApplicationCommands.SlashCommands
{
    //test
    [AttributeUsage(AttributeTargets.Method)]
    public class SlashCommand : Attribute
    {
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

        public string Name { get; }
        public string Description { get; }
        public SlashCommandOptionBuilder Options { get; }
        public ApplicationCommandOptionType CommandOptionType { get; }
    }
}