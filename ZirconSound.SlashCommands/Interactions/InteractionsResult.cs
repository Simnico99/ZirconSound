namespace ZirconSound.ApplicationCommands.Interactions;

internal class InteractionsResult : IResult
{
    public InteractionsResult(CommandError error, string reason, bool isSuccess)
    {
        Error = error;
        ErrorReason = reason;
        IsSuccess = isSuccess;
    }

    public InteractionsResult(string reason, bool isSuccess)
    {
        ErrorReason = reason;
        IsSuccess = isSuccess;
    }

    public CommandError? Error { get; }

    public string ErrorReason { get; }

    public bool IsSuccess { get; }
}
