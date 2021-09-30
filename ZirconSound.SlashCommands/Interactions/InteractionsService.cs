using Discord;
using Discord.Addons.Hosting.Util;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ZirconSound.ApplicationCommands.Component;
using ZirconSound.ApplicationCommands.Events;
using ZirconSound.ApplicationCommands.Helpers;
using ZirconSound.ApplicationCommands.SlashCommands;

namespace ZirconSound.ApplicationCommands.Interactions
{
    public class InteractionsService
    {
        private readonly AsyncEvent<Func<Optional<SocketSlashCommand>, ICommandContext, IResult, Task>> _commandExecutedEvent = new();
        private readonly AsyncEvent<Func<Optional<SocketMessageComponent>, ICommandContext, IResult, Task>> _componentExecutedEvent = new();
        private readonly InteractionsServiceConfig _config;
        private IServiceProvider _provider;
        public Func<LogMessage, Task> Log;

        public InteractionsService(InteractionsServiceConfig config) => _config = config;

        private IEnumerable<SlashCommandGroup> Commands { get; set; }


        public event Func<Optional<SocketSlashCommand>, ICommandContext, IResult, Task> CommandExecuted
        {
            add => _commandExecutedEvent.Add(value);
            remove => _commandExecutedEvent.Remove(value);
        }

        public event Func<Optional<SocketMessageComponent>, ICommandContext, IResult, Task> ComponentExecuted
        {
            add => _componentExecutedEvent.Add(value);
            remove => _componentExecutedEvent.Remove(value);
        }

        private async Task CommandBuilder(IEnumerable<SlashCommand> commands, DiscordSocketClient client)
        {
            var commandList = new List<ApplicationCommandProperties>();
            var commandToAdd = new List<SlashCommandBuilder>();

            foreach (var command in commands)
            {
                var globalCommand = new SlashCommandBuilder();
                globalCommand.WithName(command.Name);
                if (!string.IsNullOrEmpty(command.Description))
                {
                    globalCommand.WithDescription(command.Description);
                }

                if (command.Options != null)
                {
                    globalCommand.AddOption(command.Options);
                }

                await Log.Invoke(new LogMessage(_config.LogLevel, "InteractionsService", $"Adding Command: {command.Name}\n{command.Description}"));

                commandToAdd.Add(globalCommand);
                commandList.Add(globalCommand.Build());

            }

            try
            {
                var actualCommands = await client.GetGlobalApplicationCommandsAsync();

                var addCommands = actualCommands.Any(command => !commandToAdd.Any(x => x.Name == command.Name && x.Description == command.Description));

                if (addCommands)
                {
                    await client.BulkOverwriteGlobalApplicationCommandsAsync(commandList.ToArray());

                    await Log.Invoke(new LogMessage(_config.LogLevel, "InteractionsService", "Added slash commands"));
                }
                else
                {
                    await Log.Invoke(new LogMessage(LogSeverity.Warning, "InteractionsService", "Command creation got skipped:\nBecause no new commands got added!"));
                }
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                await Log.Invoke(new LogMessage(LogSeverity.Error, "InteractionsService", json));
            }
            catch (Exception ex)
            {
                await Log.Invoke(new LogMessage(LogSeverity.Error, "InteractionsService", ex.Message));
            }
        }

        private async Task ExecuteInternalAsync(SocketSlashCommand command, ICommandContext slashCommandContext)
        {
            var commandToExec = Commands.FirstOrDefault(x => x.Command.Name == command.CommandName);

            try
            {
                if (commandToExec?.CommandModule != null)
                {
                    dynamic commandClass = ActivatorUtilities.CreateInstance(_provider, commandToExec.CommandModule);

                    MethodInfo setContext = commandClass.GetType().GetMethod("SetContext");
                    var parameterArray = new object[] { slashCommandContext };
                    setContext.Invoke(commandClass, parameterArray);

                    MethodInfo theMethod = commandClass.GetType().GetMethod(commandToExec.Method.Name);
                    var secondParameterArray = Array.Empty<object>();
                    if (command.Data.Options != null)
                    {
                        secondParameterArray = new[] { command.Data.Options.First().Value };
                    }
                    else if (commandToExec.Command.CommandOptionType == ApplicationCommandOptionType.Integer)
                    {
                        secondParameterArray = new object[] { 0 };
                    }

                    theMethod.Invoke(commandClass, secondParameterArray);
                }

                await _commandExecutedEvent.InvokeAsync(command, slashCommandContext, new SlashCommandResult("Success", true)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _commandExecutedEvent.InvokeAsync(command, slashCommandContext, new SlashCommandResult(CommandError.Exception, ex.Message, true)).ConfigureAwait(false);
            }
        }

        private async Task ExecuteInternalAsync(SocketMessageComponent component, ICommandContext slashCommandContext)
        {
            var commandToExec = Commands.FirstOrDefault(x => x.Command.Name == component.Data.CustomId);

            try
            {
                if (commandToExec?.CommandModule != null)
                {
                    dynamic commandClass = ActivatorUtilities.CreateInstance(_provider, commandToExec.CommandModule);

                    MethodInfo setContext = commandClass.GetType().GetMethod("SetContext");
                    var parameterArray = new object[] { slashCommandContext };
                    setContext.Invoke(commandClass, parameterArray);

                    MethodInfo theMethod = commandClass.GetType().GetMethod(commandToExec.Method.Name);
                    var secondParameterArray = Array.Empty<object>();
                    if (component.Data.Values != null)
                    {
                        secondParameterArray = new object[] { component.Data.Values.FirstOrDefault() };
                    }
                    else if (commandToExec.Command.CommandOptionType == ApplicationCommandOptionType.Integer)
                    {
                        secondParameterArray = new object[] { 0 };
                    }

                    theMethod.Invoke(commandClass, secondParameterArray);
                }

                await _componentExecutedEvent.InvokeAsync(component, slashCommandContext, new SlashCommandResult("Success", true)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _componentExecutedEvent.InvokeAsync(component, slashCommandContext, new SlashCommandResult(CommandError.Exception, ex.Message, true)).ConfigureAwait(false);
            }
        }

        public async Task InvokeSlashCommand(SocketSlashCommand command, SlashCommandContext slashCommandContext)
        {
            await Log.Invoke(new LogMessage(_config.LogLevel, "InteractionsService", $"Executed command: {command.CommandName} User:{slashCommandContext.User.Username}/{slashCommandContext.Guild.Id}"));

            switch (_config.DefaultRunMode)
            {
                case RunMode.Sync:
                    await ExecuteInternalAsync(command, slashCommandContext).ConfigureAwait(false);
                    break;
                case RunMode.Async:
                    _ = Task.Run(async () => { await ExecuteInternalAsync(command, slashCommandContext).ConfigureAwait(false); });
                    break;
            }
        }

        public async Task InvokeComponent(SocketMessageComponent component, ComponentContext componentContext)
        {
            await Log.Invoke(new LogMessage(_config.LogLevel, "InteractionsService", $"Executed component: {component.Message} User:{componentContext.User.Username}/{componentContext.Guild.Id}"));

            switch (_config.DefaultRunMode)
            {
                case RunMode.Sync:
                    await ExecuteInternalAsync(component, componentContext).ConfigureAwait(false);
                    break;
                case RunMode.Async:
                    _ = Task.Run(async () => { await ExecuteInternalAsync(component, componentContext).ConfigureAwait(false); });
                    break;
            }
        }

        public async Task AddModuleAsync(Assembly assembly, IServiceProvider provider, DiscordSocketClient client, CancellationToken cancellationToken)
        {
            await client.WaitForReadyAsync(cancellationToken);
            _provider = provider;

            Commands = ModuleHelper.GetSlashModules(assembly);

            var commands = Commands.Select(commandsMethods => commandsMethods.Command).ToList();

            await CommandBuilder(commands, client);
        }
    }
}