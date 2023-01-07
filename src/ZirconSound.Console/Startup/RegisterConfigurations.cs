using Microsoft.Extensions.Configuration;

namespace ZirconSound.Console.Startup;
public static partial class IConfigurationBuilderExtension
{
    public static IConfigurationBuilder RegisterConfigurations(this IConfigurationBuilder configuration)
    {
        configuration.SetBasePath(Directory.GetCurrentDirectory());
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configuration.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_") ?? "Production"}.json", optional: true);
        configuration.AddEnvironmentVariables();
        configuration.Build();

        return configuration;
    }
}
