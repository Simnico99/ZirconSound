using Discord.Commands;
using Discord.Commands.Builders;

namespace ZirconSound.ApplicationCommands.SlashCommands
{
    public interface ISlashModuleBase
    {
        /// <summary>
        /// Set the context of the command.
        /// </summary>
        /// <param name="context">Context of the command.</param>
        void SetContext(ICommandContext context);

        /// <summary>
        /// Before the command get executed.
        /// </summary>
        /// <param name="command">the command.</param>
        void BeforeExecute(CommandInfo command);

        /// <summary>
        /// Before the command get executed.
        /// </summary>
        /// <param name="command">the command.</param>
        void AfterExecute(CommandInfo command);

        /// <summary>
        /// Action to do when the module gets built
        /// </summary>
        /// <param name="commandService">The command service.</param>
        /// <param name="builder">the module builder.</param>
        void OnModuleBuilding(CommandService commandService, ModuleBuilder builder);
    }
}