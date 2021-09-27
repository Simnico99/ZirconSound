using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZirconSound.SlashCommands
{
    internal class SlashCommandResult : IResult
    {
        public CommandError? Error { get; }

        public string ErrorReason { get; }

        public bool IsSuccess { get; }

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
    }
}
