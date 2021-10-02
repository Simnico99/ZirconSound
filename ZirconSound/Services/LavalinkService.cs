using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace ZirconSound.Services
{
    internal class LavalinkService
    {
        private static ILogger<LavalinkService> Client { get; set; }

        public static void Start(ILogger<LavalinkService> socketClient)
        {
            Client = socketClient;

            var path = Directory.GetCurrentDirectory();

            Process clientProcess = new();
            clientProcess.StartInfo.FileName = "java";
            clientProcess.StartInfo.Arguments = $@"-jar {path}\Lavalink\Lavalink.jar ";
            clientProcess.StartInfo.UseShellExecute = false;
            clientProcess.StartInfo.RedirectStandardOutput = true;
            clientProcess.StartInfo.RedirectStandardError = true;
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
                    Client.LogInformation(cleanMessage);
                }
                else if (message.Contains("WARN"))
                {
                    Client.LogWarning(cleanMessage);
                }
                else if (message.Contains("ERROR"))
                {
                    Client.LogError(cleanMessage);
                }
            }
        }
    }
}