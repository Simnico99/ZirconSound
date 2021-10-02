using Discord.Commands;

namespace ZirconSound.ApplicationCommands.Interactions
{
    internal class InteractionResult : IResult
    {
        public InteractionResult(CommandError error, string reason, bool isSuccess)
        {
            Error = error;
            ErrorReason = reason;
            IsSuccess = isSuccess;
        }

        public InteractionResult(string reason, bool isSuccess)
        {
            ErrorReason = reason;
            IsSuccess = isSuccess;
        }

        public CommandError? Error { get; }

        public string ErrorReason { get; }

        public bool IsSuccess { get; }
    }
}