namespace System.Threading.Tasks;

public class TaskAsync : Task
{
    private static async Task RunActionAsync(Action action) => await Task.Run(action);

    public static new Task Run(Action action) => Run((Func<Task>)(async () => await RunActionAsync(action)));

    public static new Task Run(Action action, CancellationToken cancellationToken) => Run((Func<Task>)(async () => await RunActionAsync(action)), cancellationToken);

    public TaskAsync(Action action) : base(action)
    {
    }

    public TaskAsync(Action action, CancellationToken cancellationToken) : base(action, cancellationToken)
    {
    }

    public TaskAsync(Action action, TaskCreationOptions creationOptions, CancellationToken cancellationToken) : base(action, cancellationToken, creationOptions)
    {
    }

    public TaskAsync(Action action, TaskCreationOptions creationOptions) : base(action, creationOptions)
    {
    }

    public TaskAsync(Action<object> action, object state) : base(action, state)
    {
    }

    public TaskAsync(Action<object> action, object state, CancellationToken cancellationToken) : base(action, state, cancellationToken)
    {
    }

    public TaskAsync(Action<object> action, object state, TaskCreationOptions creationOptions, CancellationToken cancellationToken) : base(action, state, cancellationToken, creationOptions)
    {
    }

    public TaskAsync(Action<object> action, object state, TaskCreationOptions creationOptions) : base(action, state, creationOptions)
    {
    }
}
