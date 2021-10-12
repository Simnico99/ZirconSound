using System.Reflection;

namespace ZirconSound.Handlers;

internal class InteractionHandler : DiscordClientService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionsService _interactionsService;
    private readonly IServiceProvider _provider;

    public InteractionHandler(DiscordSocketClient client, InteractionsService slashInteractions, ILogger<InteractionHandler> logger, IServiceProvider provider) : base(client, logger)
    {
        _provider = provider;
        _client = client;
        _interactionsService = slashInteractions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.InteractionCreated += Client_InteractionCreated;
        _interactionsService.CommandExecuted += SlashCommandHandler.Executed;
        _interactionsService.MessageComponentExecuted += MessageComponentHandler.Executed;

        await _interactionsService.AddModuleAsync(Assembly.GetExecutingAssembly(), _provider, _client, stoppingToken);
    }

    private async Task Client_InteractionCreated(SocketInteraction interaction)
    {
        await interaction.DeferAsync();
        switch (interaction)
        {
            // Slash commands/
            case SocketSlashCommand commandInteraction:
                await SlashCommandHandler.Invoke(_client, commandInteraction, _interactionsService);
                break;

            // Button clicks/selection dropdowns
            case SocketMessageComponent componentInteraction:
                await MessageComponentHandler.Invoke(_client, componentInteraction, _interactionsService);
                break;

            // Unused or Unknown/Unsupported
            default:
                await interaction.FollowupAsync("Unsupported interaction.");
                break;
        }
    }
}
