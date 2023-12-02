using System.Runtime.Serialization;

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

    public LavalinkAlreadyRunningException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public LavalinkAlreadyRunningException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
