using System.Collections.Immutable;

namespace ZirconSound.ApplicationCommands.Events;

internal class AsyncEvent<T>
    where T : class
{
    private readonly object _subLock = new();
    private ImmutableArray<T> _subscriptions;

    public AsyncEvent() => _subscriptions = ImmutableArray.Create<T>();

    public bool HasSubscribers => _subscriptions.Length != 0;
    public IReadOnlyList<T> SubscriptionsList => _subscriptions;

    public void Add(T subscriber)
    {
        Preconditions.NotNull(subscriber, nameof(subscriber));
        lock (_subLock)
        {
            _subscriptions = _subscriptions.Add(subscriber);
        }
    }

    public void Remove(T subscriber)
    {
        Preconditions.NotNull(subscriber, nameof(subscriber));
        lock (_subLock)
        {
            _subscriptions = _subscriptions.Remove(subscriber);
        }
    }
}

internal static class EventExtensions
{
    public static async Task InvokeAsync<T1, T2, T3>(this AsyncEvent<Func<T1, T2, T3, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3)
    {
        var subscribers = eventHandler.SubscriptionsList;
        for (var i = 0; i < subscribers.Count; i++)
        {
            await subscribers[i].Invoke(arg1, arg2, arg3).ConfigureAwait(false);
        }
    }
}
