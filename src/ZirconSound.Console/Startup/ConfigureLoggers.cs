using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;

namespace ZirconSound.Console.Startup;
public static partial class IHostBuilderExtension
{
    public static IHostBuilder ConfigureLoggers(this IHostBuilder builder, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
        .ReadFrom
        .Configuration(configuration)
        .CreateLogger();

        Log.Information("Starting {SoftwareName} up!", AppDomain.CurrentDomain.FriendlyName);
        Log.Information("Environment: {Environment}", Environment.GetEnvironmentVariable("DOTNET_") ?? "Production");
        Log.Information("Version: {CurrentVersion}", Assembly.GetExecutingAssembly().GetName().Version);

        return builder;
    }
}
