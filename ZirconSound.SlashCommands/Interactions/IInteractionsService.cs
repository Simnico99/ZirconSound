namespace ZirconSound.ApplicationCommands.Interactions;

public interface IInteractionsService
{
    IEnumerable<SlashCommandGroup> SlashCommands { get; }

    event Func<Optional<SocketSlashCommand>, IInteractionContext, IResult, Task> CommandExecuted;
    event Func<Optional<SocketMessageComponent>, IInteractionContext, IResult, Task> MessageComponentExecuted;

    Task AddModuleAsync(Assembly assembly, IServiceProvider provider, DiscordSocketClient client, CancellationToken cancellationToken);
    Task Invoke(SocketInteraction component, IInteractionContext context);
}
