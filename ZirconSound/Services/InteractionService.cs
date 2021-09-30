using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ZirconSound.ApplicationCommands.Interactions;
using ZirconSound.Handlers;

namespace ZirconSound.Services
{
    internal class InteractionService : DiscordClientService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionsService _interactionsService;
        private readonly IServiceProvider _provider;

        public InteractionService(DiscordSocketClient client, InteractionsService slashInteractions, ILogger<InteractionService> logger, IServiceProvider provider) : base(client, logger)
        {
            _provider = provider;
            _client = client;
            _interactionsService = slashInteractions;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.InteractionCreated += Client_InteractionCreated;
            _interactionsService.CommandExecuted += SlashCommandHandler.InteractionsExecuted;
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
                    //await MyMessageComponentHandler(componentInteraction);
                    break;

                // Unused or Unknown/Unsupported
                default:
                    await interaction.FollowupAsync("Unsupported interaction.");
                    break;
            }
        }
    }
}