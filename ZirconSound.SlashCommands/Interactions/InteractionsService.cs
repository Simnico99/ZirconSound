using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting.Util;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ZirconSound.ApplicationCommands.Events;
using ZirconSound.ApplicationCommands.Helpers;
using ZirconSound.ApplicationCommands.MessageComponents;
using ZirconSound.ApplicationCommands.SlashCommands;

namespace ZirconSound.ApplicationCommands.Interactions
{
    public class InteractionsService
    {
        private readonly AsyncEvent<Func<Optional<SocketSlashCommand>, IInteractionContext, IResult, Task>> _commandExecutedEvent = new();
        private readonly AsyncEvent<Func<Optional<SocketMessageComponent>, IInteractionContext, IResult, Task>> _messageComponentExecutedEvent = new();
        private readonly InteractionsServiceConfig _config;
        private IServiceProvider _provider;
        public Func<LogMessage, Task> Log;

        public InteractionsService(InteractionsServiceConfig config) => _config = config;

        public IEnumerable<SlashCommandGroup> SlashCommands { get; private set; }
        private IEnumerable<MessageComponentGroup> MessageComponents { get; set; }

        public event Func<Optional<SocketSlashCommand>, IInteractionContext, IResult, Task> CommandExecuted
        {
            add => _commandExecutedEvent.Add(value);
            remove => _commandExecutedEvent.Remove(value);
        }

        public event Func<Optional<SocketMessageComponent>, IInteractionContext, IResult, Task> MessageComponentExecuted
        {
            add => _messageComponentExecutedEvent.Add(value);
            remove => _messageComponentExecutedEvent.Remove(value);
        }

        private async Task CommandBuilder(IEnumerable<SlashCommandAttribute> commands, DiscordSocketClient client)
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

        private static void SetSlashCommandParameters(ref object[] parameterArray, InteractionData interactionData, IInteractionGroup commandToExec)
        {
            if (commandToExec is SlashCommandGroup command)
            {
                if (interactionData.Data?.GetType().GetProperty("Options") != null)
                {
                    if (interactionData.Data.Options != null)
                    {
                        var length = interactionData.Data.Options.Length;
                        parameterArray = new object[length];
                        for (var runs = 0; runs < length; runs++)
                        {
                            parameterArray[runs] = interactionData.Data.Options[runs].Value.ToString();
                        }
                    }
                    else if (command.Interaction.CommandOptionType == ApplicationCommandOptionType.Integer)
                    {
                        parameterArray = new object[] { 0 };
                    }
                }
            }
        }

        private static void SetMessageComponentParameters(ref object[] parameterArray, InteractionData interactionData, MethodBase method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length > 0)
            {
                var i = 0;
                foreach (var parameter in parameters)
                {
                    var paramType = parameter.ParameterType;
                    if (interactionData.Data.Length == parameters.Length)
                    {
                        if (interactionData.Data[i] is not null)
                        {
                            var converted = TypeDescriptor.GetConverter(paramType).ConvertFromString(interactionData.Data[i]);
                            interactionData.Data[i] = converted;
                        }
                        else
                        {
                            if (paramType == typeof(long) || paramType == typeof(int) || paramType == typeof(double))
                            {
                                interactionData.Data[i] = 0;
                            }
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            interactionData.Data = new object[parameters.Length];
                        }
                        if (paramType == typeof(long) || paramType == typeof(int) || paramType == typeof(double))
                        {
                            interactionData.Data[i] = 0;
                        }
                    }

                    i++;
                }
                parameterArray = interactionData.Data;
            }
        }

        private async Task<KeyValuePair<InteractionData, IInteractionGroup>> GetDataFromSocket(SocketInteraction interaction)
        {
            IInteractionGroup commandToExec = new InteractionGroup();
            var interactionData = new InteractionData();
            switch (interaction)
            {
                case SocketSlashCommand commandInteraction:
                    interactionData.Data = commandInteraction.Data;
                    interactionData.Name = commandInteraction.CommandName;
                    interactionData.Type = InteractionType.SlashCommand;
                    commandToExec = SlashCommands.FirstOrDefault(x => x.Interaction.Name == interactionData.Name);
                    break;

                case SocketMessageComponent componentInteraction:
                    var dataArray = Array.Empty<object>();
                    if (componentInteraction.Data.CustomId.Contains(";"))
                    {
                        var splitData = componentInteraction.Data.CustomId.Split(";");
                        interactionData.Name = splitData[0];
                        var length = splitData.Length;
                        dataArray = new object[length - 1];
                        for (var runs = 1; runs < length; runs++)
                        {
                            dataArray[runs - 1] = splitData[runs];
                        }
                    }
                    else
                    {
                        interactionData.Name = componentInteraction.Data.CustomId;
                    }

                    interactionData.Data = dataArray;

                    interactionData.Type = InteractionType.MessageComponent;
                    commandToExec = MessageComponents.FirstOrDefault(x => x.Interaction.Id == interactionData.Name);
                    break;

                default:
                    await interaction.FollowupAsync("Unsupported interaction.");
                    break;
            }
            return new KeyValuePair<InteractionData, IInteractionGroup>(interactionData, commandToExec);
        }

        private async Task ExecuteInternalAsync(SocketInteraction interaction, IInteractionContext context)
        {
            try
            {
                var (interactionData, commandToExec) = await GetDataFromSocket(interaction);

                if (commandToExec?.Module != null)
                {
                    var commandClass = ActivatorUtilities.CreateInstance(_provider, commandToExec.Module);

                    var setContext = commandClass.GetType().GetMethod("SetContext");
                    var parameterArray = new object[] { context };
                    setContext?.Invoke(commandClass, parameterArray);

                    var method = commandClass.GetType().GetMethod(commandToExec.Method.Name);
                    var secondParameterArray = Array.Empty<object>();
                    if (interactionData.Type == InteractionType.SlashCommand)
                    {
                        SetSlashCommandParameters(ref secondParameterArray, interactionData, commandToExec);
                    }
                    else
                    {
                        SetMessageComponentParameters(ref secondParameterArray, interactionData, method);
                    }

                    method?.Invoke(commandClass, secondParameterArray);
                }

                if (interactionData.Type == InteractionType.SlashCommand)
                {
                    await _commandExecutedEvent.InvokeAsync(interaction as SocketSlashCommand, context, new InteractionResult("Success", true)).ConfigureAwait(false);
                }
                else
                {
                    await _messageComponentExecutedEvent.InvokeAsync(interaction as SocketMessageComponent, context, new InteractionResult("Success", true)).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (interaction is SocketSlashCommand command)
                {
                    await _commandExecutedEvent.InvokeAsync(command, context, new InteractionResult(CommandError.Exception, ex.Message, false)).ConfigureAwait(false);
                }
                else
                {
                    await _messageComponentExecutedEvent.InvokeAsync(interaction as SocketMessageComponent, context, new InteractionResult(CommandError.Exception, ex.Message, false)).ConfigureAwait(false);
                }
            }
        }

        public async Task Invoke(SocketInteraction component, IInteractionContext context)
        {
            await Log.Invoke(new LogMessage(_config.LogLevel, "InteractionsService", $"Executed Interaction: User:{context.User.Username}/{context.Guild.Id}"));

            switch (_config.DefaultRunMode)
            {
                case RunMode.Sync:
                    await ExecuteInternalAsync(component, context).ConfigureAwait(false);
                    break;
                case RunMode.Async:
                    _ = Task.Run(async () => { await ExecuteInternalAsync(component, context).ConfigureAwait(false); });
                    break;
                case RunMode.Default:
                    await ExecuteInternalAsync(component, context).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_config.DefaultRunMode), "This run mode is not supported!");
            }
        }

        public async Task AddModuleAsync(Assembly assembly, IServiceProvider provider, DiscordSocketClient client, CancellationToken cancellationToken)
        {
            await client.WaitForReadyAsync(cancellationToken);
            _provider = provider;

            SlashCommands = ModuleHelper.GetInteractionModules<SlashCommandGroup, SlashCommandAttribute>(assembly);
            MessageComponents = ModuleHelper.GetInteractionModules<MessageComponentGroup, MessageComponentAttribute>(assembly);

            var commands = SlashCommands.Select(commandsMethods => commandsMethods.Interaction).ToList();

            await CommandBuilder(commands, client);
        }
    }
}