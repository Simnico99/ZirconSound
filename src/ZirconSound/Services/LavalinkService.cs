using System.Diagnostics;

namespace ZirconSound.Services;

internal class LavalinkService
{
    private static ILogger<LavalinkService> Logger { get; set; }
    private static Process _process;
    public static EventWaitHandle IsReady { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);



    public static void Start(ILogger<LavalinkService> logger)
    {
        Logger = logger;

        var path = Directory.GetCurrentDirectory();

        _process = new();
        _process.StartInfo = new ProcessStartInfo
        {
            FileName = "java",
            Arguments = $@"-jar {path}\Lavalink\Lavalink.jar ",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        //* Set your output and error (asynchronous) handlers
        _process.OutputDataReceived += OutputHandler;
        _process.ErrorDataReceived += OutputHandler;
        //* Start process and handlers
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
    }

    public static void CloseProcess() 
    {
        _process?.Kill(true);
    }

    private static void OutputHandler(object sender, DataReceivedEventArgs e)
    {
        var message = e.Data;
        if (!string.IsNullOrEmpty(message))
        {
            var lastIndexOf = message.LastIndexOf(']');
            var cleanMessage = message;

            if (lastIndexOf > 0)
            {
                cleanMessage = message[(lastIndexOf + 1)..];
            }

            if (message.Contains("INFO"))
            {
                Logger.LogInformation("Lavalink Jar: {Message}", cleanMessage);
            }
            else if (message.Contains("WARN"))
            {
                Logger.LogWarning("Lavalink Jar: {Message}", cleanMessage);
            }
            else if (message.Contains("ERROR"))
            {
                Logger.LogError("Lavalink Jar: {Message}", cleanMessage);
            }

            if (message.Contains("https://github.com/Frederikam/Lavalink/issues/295"))
            {
                IsReady.Set();
            }
        }
    }
}
