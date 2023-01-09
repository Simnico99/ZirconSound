namespace ZirconSound.Application.Interfaces;

public interface ILavalinkRunnerService
{
    EventWaitHandle IsReady { get; }
}