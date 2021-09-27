using Discord.Commands;

namespace ZirconSound.ApplicationCommands.SlashCommands
{
    internal class SlashCommandResult : IResult
    {
        public SlashCommandResult(CommandError error, string reason, bool isSuccess)
        {
            Error = error;
            ErrorReason = reason;
            IsSuccess = isSuccess;
        }

        public SlashCommandResult(string reason, bool isSuccess)
        {
            ErrorReason = reason;
            IsSuccess = isSuccess;
        }

        public CommandError? Error { get; }

        public string ErrorReason { get; }

        public bool IsSuccess { get; }
    }
}