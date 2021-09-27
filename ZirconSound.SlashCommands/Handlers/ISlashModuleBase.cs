using Discord.Commands;
using Discord.Commands.Builders;

namespace ZirconSound.SlashCommands.Handlers
{
    public interface ISlashModuleBase
    {
        void SetContext(ICommandContext context);

        void BeforeExecute(CommandInfo command);

        void AfterExecute(CommandInfo command);

        void OnModuleBuilding(CommandService commandService, ModuleBuilder builder);
    }
}