using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using System.Reflection;

namespace ZirconSound.Handlers;

internal class InteractionHandler : DiscordClientService
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _provider;
    private readonly InteractionService _interactionService;
    private readonly IConfiguration _configuration;

    public InteractionHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceProvider provider, InteractionService interactionService, IConfiguration configuration) : base(client, logger)
    {
        _logger = logger;
        _provider = provider;
        _interactionService = interactionService;
        _configuration = configuration;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.InteractionCreated += HandleInteraction;

        // Process the command execution results 
        _interactionService.SlashCommandExecuted += SlashCommandExecuted;
        _interactionService.ContextCommandExecuted += ContextCommandExecuted;
        _interactionService.ComponentCommandExecuted += ComponentCommandExecuted;

        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        await Client.WaitForReadyAsync(stoppingToken);


        // If DOTNET_ENVIRONMENT is set to development, only register the commands to a single guild
        if (Environment.GetEnvironmentVariable("DOTNET_") == "Development")
        {
            var devGuild = _configuration.GetValue<ulong>("devguild");
            Logger.LogWarning("Registering commands to GUILD: {guild}", devGuild);
            await _interactionService.RegisterCommandsToGuildAsync(devGuild);
            Logger.LogWarning("Registered commands.");
        }
        else
        {
            await _interactionService.RegisterCommandsGloballyAsync();
        }
    }

    private async Task ComponentCommandExecuted(ComponentCommandInfo componentCommandInfo, IInteractionContext context, Discord.Interactions.IResult result)
    {
        if (!result.IsSuccess)
        {
            var embeds = EmbedHandler.Create(context);
            embeds.WithDescription(result.ErrorReason);
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    embeds.WithTitle("Unmet Precondition");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                case InteractionCommandError.UnknownCommand:
                    embeds.WithTitle("Unkown Command");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                case InteractionCommandError.BadArgs:
                    embeds.WithTitle("Bad Argument(s)");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                case InteractionCommandError.Exception:
                    embeds.WithTitle("Error");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Error));
                    break;
                case InteractionCommandError.Unsuccessful:
                    embeds.WithTitle("Unsuccessful");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                default:
                    break;
            }
        }
    }

    private async Task ContextCommandExecuted(ContextCommandInfo contextCommandInfo, IInteractionContext context, Discord.Interactions.IResult result)
    {
        if (!result.IsSuccess)
        {
            var embeds = EmbedHandler.Create(context);
            embeds.WithDescription(result.ErrorReason);
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    embeds.WithTitle("Unmet Precondition");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                case InteractionCommandError.UnknownCommand:
                    embeds.WithTitle("Unkown Command");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                case InteractionCommandError.BadArgs:
                    embeds.WithTitle("Bad Argument(s)");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                case InteractionCommandError.Exception:
                    embeds.WithTitle("Error");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Error));
                    break;
                case InteractionCommandError.Unsuccessful:
                    embeds.WithTitle("Unsuccessful");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                default:
                    break;
            }
        }
    }

    private async Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext context, Discord.Interactions.IResult result)
    {
        if (!result.IsSuccess)
        {
            var embeds = EmbedHandler.Create(context);
            embeds.WithDescription(result.ErrorReason);
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    embeds.WithTitle("Unmet Precondition");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                case InteractionCommandError.UnknownCommand:
                    embeds.WithTitle("Unkown Command");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                case InteractionCommandError.BadArgs:
                    embeds.WithTitle("Bad Argument(s)");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
                    break;
                case InteractionCommandError.Exception:
                    embeds.WithTitle("Error");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Error));
                    break;
                case InteractionCommandError.Unsuccessful:
                    embeds.WithTitle("Unsuccessful");
                    await context.ReplyToCommandAsync(embed: embeds.BuildSync(ZirconEmbedType.Warning));
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
            await arg.RespondAsync("Processing...");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "An exception happened during the answer defering:");
        }
        
        Logger.LogInformation("{UserName} executed: {Command}", arg.User.Username, arg.Data.ToString() ?? null);


        try
        {
            var ctx = new SocketInteractionContext(Client, arg);
            await _interactionService.ExecuteCommandAsync(ctx, _provider);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred whilst attempting to handle interaction.");

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                var msg = await arg.GetOriginalResponseAsync();
                await msg.DeleteAsync();
            }

        }
    }
}
