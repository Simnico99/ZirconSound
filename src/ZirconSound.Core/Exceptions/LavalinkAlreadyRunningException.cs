namespace ZirconSound.Core.Exceptions;

[Serializable]
public sealed class LavalinkAlreadyRunningException : Exception
{
    public LavalinkAlreadyRunningException()
    {
    }

    public LavalinkAlreadyRunningException(string? message) : base(message)
    {
    }


    public LavalinkAlreadyRunningException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
