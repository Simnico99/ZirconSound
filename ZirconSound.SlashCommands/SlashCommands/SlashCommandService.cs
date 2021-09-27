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
using ZirconSound.ApplicationCommands.Events;

namespace ZirconSound.ApplicationCommands.SlashCommands
{
    public class SlashCommandService
    {
        private readonly AsyncEvent<Func<Optional<SocketSlashCommand>, ICommandContext, IResult, Task>> _commandExecutedEvent = new();
        private readonly SlashCommandServiceConfig _config;
        private IServiceProvider _provider;
        public Func<LogMessage, Task> Log;

        public SlashCommandService(SlashCommandServiceConfig config) => _config = config;

        private IEnumerable<SlashCommandGroup> Commands { get; set; }

        /*
        public event Func<SocketSlashCommand, Task> MessageReceived { add => _messageReceivedEvent.Add(value);
            remove => _messageReceivedEvent.Remove(value);
        }
        private readonly LocalAsyncEvent<Func<SocketSlashCommand, Task>> _messageReceivedEvent = new();
        */
        public event Func<Optional<SocketSlashCommand>, ICommandContext, IResult, Task> CommandExecuted
        {
            add => _commandExecutedEvent.Add(value);
            remove => _commandExecutedEvent.Remove(value);
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

                await Log.Invoke(new LogMessage(_config.LogLevel, "SlashCommandService", $"Adding Command: {command.Name}\n{command.Description}"));

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

                    await Log.Invoke(new LogMessage(_config.LogLevel, "SlashCommandService", "Added slash commands"));
                }
                else
                {
                    await Log.Invoke(new LogMessage(LogSeverity.Warning, "SlashCommandService", "Command creation got skipped:\nBecause no new commands got added!"));
                }
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                await Log.Invoke(new LogMessage(LogSeverity.Error, "SlashCommandService", json));
            }
            catch (Exception ex)
            {
                await Log.Invoke(new LogMessage(LogSeverity.Error, "SlashCommandService", ex.Message));
            }
        }

        private async Task ExecuteInternalAsync(SocketSlashCommand command, SlashCommandContext slashCommandContext)
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

        public async Task Invoke(SocketSlashCommand command, SlashCommandContext slashCommandContext)
        {
            await Log.Invoke(new LogMessage(_config.LogLevel, "SlashCommandService", $"Executed command: {command.CommandName} User:{slashCommandContext.User.Username}/{slashCommandContext.Guild.Id}"));

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

        public async Task AddModuleAsync(Assembly assembly, IServiceProvider provider, DiscordSocketClient client, CancellationToken cancellationToken)
        {
            await client.WaitForReadyAsync(cancellationToken);
            _provider = provider;

            Commands = SlashCommandHelper.GetSlashCommands(assembly);

            var commands = Commands.Select(commandsMethods => commandsMethods.Command).ToList();

            await CommandBuilder(commands, client);
        }
    }
}