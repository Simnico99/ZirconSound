using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using ZirconSound.Core.Helpers;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Enums;
using Microsoft.Extensions.Hosting;
using Discord.Addons.Hosting.Util;

namespace ZirconSound.Application.Handlers;
public sealed class InteractionHandler : BackgroundService

{
    private readonly ILogger _logger;
    private readonly IServiceProvider _provider;
    private readonly IConfiguration _configuration;

    private readonly DiscordSocketClient _discordSocketClient;
    private readonly InteractionService _interactionService;

    public InteractionHandler(DiscordSocketClient discordSocketClient, ILogger<InteractionHandler> logger, IServiceProvider provider, InteractionService interactionService, IConfiguration configuration)
    {
        _logger = logger;
        _provider = provider;
        _interactionService = interactionService;
        _configuration = configuration;
        _discordSocketClient = discordSocketClient;
    }


    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordSocketClient.InteractionCreated += HandleInteraction;

        // Process the command execution results 
        _interactionService.SlashCommandExecuted += CommandExecuted;
        _interactionService.ContextCommandExecuted += CommandExecuted;
        _interactionService.ComponentCommandExecuted += CommandExecuted;

        await _interactionService.AddModulesAsync(Assembly.GetAssembly(typeof(InteractionHandler)), _provider);
        await _discordSocketClient.WaitForReadyAsync(stoppingToken);


        // If DOTNET_ENVIRONMENT is set to development, only register the commands to a single guild
        if (Environment.GetEnvironmentVariable("DOTNET_") == "development")
        {
            var devGuild = _configuration.GetValue<ulong>("devguild");
            _logger.LogWarning("Registering commands to GUILD: {guild}", devGuild);
            await _interactionService.RegisterCommandsToGuildAsync(devGuild);
            _logger.LogWarning("Registered commands.");
        }
        else
        {
            await _interactionService.RegisterCommandsGloballyAsync();
        }
    }

    private static async Task CommandExecuted<TParameter>(CommandInfo<TParameter> componentCommandInfo, IInteractionContext context, IResult result) where TParameter : class, IParameterInfo
    {
        if (!result.IsSuccess)
        {
            var embeds = EmbedHelpers.CreateGenericEmbedBuilder(context);
            embeds.WithDescription(result.ErrorReason);
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    embeds.WithTitle("Unmet Precondition");
                    await context.ReplyToLastCommandAsync(embed: embeds.Build(GenericEmbedType.Warning));
                    break;
                case InteractionCommandError.UnknownCommand:
                    embeds.WithTitle("Unkown Command");
                    await context.ReplyToLastCommandAsync(embed: embeds.Build(GenericEmbedType.Warning));
                    break;
                case InteractionCommandError.BadArgs:
                    embeds.WithTitle("Bad Argument(s)");
                    await context.ReplyToLastCommandAsync(embed: embeds.Build(GenericEmbedType.Warning));
                    break;
                case InteractionCommandError.Exception:
                    embeds.WithTitle("Error");
                    await context.ReplyToLastCommandAsync(embed: embeds.Build(GenericEmbedType.Error));
                    break;
                case InteractionCommandError.Unsuccessful:
                    embeds.WithTitle("Unsuccessful");
                    await context.ReplyToLastCommandAsync(embed: embeds.Build(GenericEmbedType.Warning));
                    break;
                default:
                    break;
            }
        }
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            await arg.DeferAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "An exception happened during the answer defering:");
        }

        _logger.LogInformation("{UserName} executed: {Command}", arg.User.Username, arg.Data.ToString() ?? null);


        try
        {
            var ctx = new InteractionContext(_discordSocketClient, arg);
            await _interactionService.ExecuteCommandAsync(ctx, _provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred whilst attempting to handle interaction.");

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                var msg = await arg.GetOriginalResponseAsync();
                await msg.DeleteAsync();
            }

        }
    }
}
