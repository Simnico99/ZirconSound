namespace ZirconSound.ApplicationCommands.Events;

internal static class Preconditions
{
    //Objects
    /// <exception cref="ArgumentNullException"><paramref name="obj" /> must not be <see langword="null" />.</exception>
    public static void NotNull<T>(T obj, string name, string msg = null) where T : class
    {
        if (obj == null)
        {
            throw CreateNotNullException(name, msg);
        }
    }

    private static ArgumentNullException CreateNotNullException(string name, string msg)
    {
        return msg == null ? new ArgumentNullException(name) : new ArgumentNullException(name, msg);
    }
}
