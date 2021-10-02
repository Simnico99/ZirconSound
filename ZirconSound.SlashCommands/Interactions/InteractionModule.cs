using System;
using Discord.Commands;
using Discord.Commands.Builders;

namespace ZirconSound.ApplicationCommands.Interactions
{
    public abstract class InteractionModule : InteractionModule<IInteractionContext>
    {
    }

    public abstract class InteractionModule<T> : IInteractionsModuleBase where T : class, IInteractionContext
    {
        protected T Context { get; private set; }

        void IInteractionsModuleBase.SetContext(ICommandContext context)
        {
            var newValue = context as T;
            Context = newValue ?? throw new InvalidOperationException($"Invalid context type. Expected {typeof(T).Name}, got {context.GetType().Name}.");
        }

        void IInteractionsModuleBase.BeforeExecute(CommandInfo command) => BeforeExecute(command);

        void IInteractionsModuleBase.AfterExecute(CommandInfo command) => AfterExecute(command);

        void IInteractionsModuleBase.OnModuleBuilding(CommandService commandService, ModuleBuilder builder) => OnModuleBuilding(commandService, builder);

        public void SetContext(IInteractionContext context)
        {
            var newValue = context as T;
            Context = newValue ?? throw new InvalidOperationException($"Invalid context type. Expected {typeof(T).Name}, got {context.GetType().Name}.");
        }

        protected virtual void BeforeExecute(CommandInfo command)
        {
        }

        protected virtual void AfterExecute(CommandInfo command)
        {
        }

        protected virtual void OnModuleBuilding(CommandService commandService, ModuleBuilder builder)
        {
        }
    }
}