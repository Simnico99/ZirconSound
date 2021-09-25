using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
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

namespace ZirconSound.SlashCommands
{
    public class SlashCommandService : DiscordClientService
    {
        private IEnumerable<SlashCommandGroup> Commands { get; set; }
        private readonly IServiceProvider _provider;

        public SlashCommandService(DiscordSocketClient client, ILogger<SlashCommandService> logger, IServiceProvider provider) : base(client, logger)
        {
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.InteractionCreated += Client_InteractionCreated;
            await Client.WaitForReadyAsync(stoppingToken);

            var commands = new List<SlashCommand>();
            Commands = SlashCommandHelper.GetSlashCommands(Assembly.GetEntryAssembly());

            foreach (var commandsMethods in Commands)
            {
                commands.Add(commandsMethods.Command);
            }

            await CommandBuilder(commands);
        }

        public async Task CommandBuilder(IEnumerable<SlashCommand> commands)
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

                Logger.LogDebug("Adding Command: " + command.Name + "\n" + command.Description);

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
                await Client.BulkOverwriteGlobalApplicationCommandsAsync(CommandList.ToArray());
                Logger.LogDebug("Added slash commands");
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);

                Logger.LogError(json);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        
        {
            Logger.LogDebug("Recieved an interaction");
            await arg.DeferAsync();
            try
            {
                if (arg is SocketSlashCommand command)
                {
                    _ = Task.Run(async () =>
                    {
                        Logger.LogDebug($"Executing {command.CommandName} / UserName:{command.User.Username}");

                        var commandToExec = Commands.FirstOrDefault(x => x.Command.Name == command.CommandName);

                        var slashCommandContext = new SlashCommandContext(Client, command);

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
                        theMethod.Invoke(commandClass, secondparameterArray);

                        await Task.Delay(0);
                    });
                }
            }
            catch (Exception ex)
            {
                await arg.FollowupAsync("Error: " + ex.Message);
            }
        }

    }
}
