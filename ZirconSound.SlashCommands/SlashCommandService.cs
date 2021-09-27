using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ZirconSound.SlashCommands.Events;

namespace ZirconSound.SlashCommands
{
    public class SlashCommandService
    {
        private IEnumerable<SlashCommandGroup> Commands { get; set; }
        private IServiceProvider _provider;
        private readonly SlashCommmandServiceConfig _config;
        public Func<LogMessage, Task> Log;

        public event Func<SocketSlashCommand, Task> MessageReceived { add { _messageReceivedEvent.Add(value); } remove { _messageReceivedEvent.Remove(value); } }
        internal readonly LocalAsyncEvent<Func<SocketSlashCommand, Task>> _messageReceivedEvent = new();
        public event Func<Optional<SocketSlashCommand>, ICommandContext, IResult, Task> CommandExecuted { add { _commandExecutedEvent.Add(value); } remove { _commandExecutedEvent.Remove(value); } }
        internal readonly LocalAsyncEvent<Func<Optional<SocketSlashCommand>, ICommandContext, IResult, Task>> _commandExecutedEvent = new();

        public SlashCommandService(SlashCommmandServiceConfig config)
        {
            _config = config;
        }

        private async Task CommandBuilder(IEnumerable<SlashCommand> commands, DiscordSocketClient client)
        {
            var CommandList = new List<SlashCommandProperties>();

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

                CommandList.Add(globalCommand.Build());
            }

            try
            {
                //await Client.BulkOverwriteGlobalApplicationCommandsAsync(CommandList.ToArray());
                /*
                foreach (var command in CommandList)
                {
                    foreach (var guild in Client.Guilds)
                    {
                        await Client.Rest.CreateGuildCommand(command, guild.Id);
                    }
                }
                */
                await client.BulkOverwriteGlobalApplicationCommandsAsync(CommandList.ToArray());

                await Log.Invoke(new LogMessage(_config.LogLevel, "SlashCommandService", "Added slash commands"));
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
                dynamic commandClass = ActivatorUtilities.CreateInstance(_provider, commandToExec.CommandModule);

                MethodInfo setContext = commandClass.GetType().GetMethod("SetContext");
                var parameterArray = new object[] { slashCommandContext };
                setContext.Invoke(commandClass, parameterArray);

                MethodInfo theMethod = commandClass.GetType().GetMethod(commandToExec.Method.Name);
                var secondparameterArray = Array.Empty<object>();
                if (command.Data.Options != null)
                {
                    secondparameterArray = new object[] { command.Data.Options.First().Value };
                }
                else if (commandToExec.Command.CommandOptionType == ApplicationCommandOptionType.Integer)
                {
                    secondparameterArray = new object[] { 0 };
                }
                theMethod.Invoke(commandClass, secondparameterArray);

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
                    var t2 = Task.Run(async () =>
                    {
                        await ExecuteInternalAsync(command, slashCommandContext).ConfigureAwait(false);
                    });
                    break;
            }
        }

        public async Task AddModuleAsync(Assembly assembly, IServiceProvider provider, DiscordSocketClient client, CancellationToken cancellationToken)
        {
            await client.WaitForReadyAsync(cancellationToken);
            _provider = provider;

            var commands = new List<SlashCommand>();
            Commands = SlashCommandHelper.GetSlashCommands(assembly);

            foreach (var commandsMethods in Commands)
            {
                commands.Add(commandsMethods.Command);
            }

            await CommandBuilder(commands, client);
        }
    }
}
