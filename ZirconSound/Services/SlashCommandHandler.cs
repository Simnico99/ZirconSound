using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using ZirconSound.Embeds;
using ZirconSound.Enum;
using ZirconSound.SlashCommands;

namespace ZirconSound.Services
{
    internal class SlashCommandHandler : DiscordClientService
    {
        private readonly DiscordSocketClient _client;
        private readonly SlashCommandService _commandService;
        private readonly IServiceProvider _provider;

        public SlashCommandHandler(DiscordSocketClient client, SlashCommandService slashCommand, ILogger<SlashCommandHandler> logger, IServiceProvider provider) : base(client, logger)
        {
            _provider = provider;
            _client = client;
            _commandService = slashCommand;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.InteractionCreated += Client_InteractionCreated;
            _commandService.CommandExecuted += CommandExecuted;
            await _commandService.AddModuleAsync(Assembly.GetExecutingAssembly(), _provider, _client, stoppingToken);
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {
            await arg.DeferAsync();
            if (arg is SocketSlashCommand command)
            {
                var commandContext = new SlashCommandContext(_client, command);
                await _commandService.Invoke(command, commandContext);
            }
        }

        private static async Task CommandExecuted(Optional<SocketSlashCommand> commandInfo, ICommandContext commandContext, IResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            var embed = EmbedHandler.Create(commandContext);
            embed.AddField("Error:", result.ErrorReason);
            await commandInfo.Value.FollowupAsync(embed: embed.BuildSync(ZirconEmbedType.Error));
        }
    }
}