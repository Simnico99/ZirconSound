﻿using Discord.Commands;
using Discord.Commands.Builders;
using System;

namespace ZirconSound.SlashCommands
{
    public abstract class SlashModuleBase : SlashModuleBase<ICommandContext> { }

    public abstract class SlashModuleBase<T> : ISlashModuleBase where T : class, ICommandContext
    {
        public T Context { get; private set; }

        public void SetContext(ICommandContext context)
        {
            var newValue = context as T;
            Context = newValue ?? throw new InvalidOperationException($"Invalid context type. Expected {typeof(T).Name}, got {context.GetType().Name}.");
        }

        void ISlashModuleBase.SetContext(ICommandContext context)
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

        void ISlashModuleBase.BeforeExecute(CommandInfo command) => BeforeExecute(command);
        void ISlashModuleBase.AfterExecute(CommandInfo command) => AfterExecute(command);
        void ISlashModuleBase.OnModuleBuilding(CommandService commandService, ModuleBuilder builder) => OnModuleBuilding(commandService, builder);
    }
}