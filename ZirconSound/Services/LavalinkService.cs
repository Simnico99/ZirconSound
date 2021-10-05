using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ZirconSound.Services
{
    internal class LavalinkService
    {
        private static ILogger<LavalinkService> Logger { get; set; }

        public static void Start(ILogger<LavalinkService> logger)
        {
            Logger = logger;

            var path = Directory.GetCurrentDirectory();

            Process clientProcess = new();
            clientProcess.StartInfo = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = $@"-jar {path}\Lavalink\Lavalink.jar ",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            //* Set your output and error (asynchronous) handlers
            clientProcess.OutputDataReceived += OutputHandler;
            clientProcess.ErrorDataReceived += OutputHandler;
            //* Start process and handlers
            clientProcess.Start();
            clientProcess.BeginOutputReadLine();
            clientProcess.BeginErrorReadLine();
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
                    Logger.LogInformation(cleanMessage);
                }
                else if (message.Contains("WARN"))
                {
                    Logger.LogWarning(cleanMessage);
                }
                else if (message.Contains("ERROR"))
                {
                    Logger.LogError(cleanMessage);
                }
            }
        }
    }
}