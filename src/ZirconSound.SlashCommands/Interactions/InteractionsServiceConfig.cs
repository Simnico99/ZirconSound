namespace ZirconSound.ApplicationCommands.Interactions;

public class InteractionsServiceConfig
{
    public RunMode DefaultRunMode { get; set; } = RunMode.Sync;
    public LogSeverity LogLevel { get; set; } = LogSeverity.Info;
}
