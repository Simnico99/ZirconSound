using Serilog;

namespace ZirconSound;

/// <summary>
///     The entry point of the bot.
/// </summary>
internal static class Program
{
    private static async Task Main()
    {
        var exitCode = 0;
        var startup = new StartupService();
        try
        {
            await startup.Start();
        }
        catch (Exception ex)
        {
            Log.Fatal("Software crashed! Error: {Exception}", ex);
            exitCode = ex.HResult;
        }
        finally
        {
            Log.CloseAndFlush();
            startup.Dispose();

            Environment.Exit(exitCode);
        }
    }
}
