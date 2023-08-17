using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using ZirconSound.Console.Startup;

try
{
    //Create
    var host = Host.CreateDefaultBuilder(args);
    var config = new ConfigurationBuilder().RegisterConfigurations().Build();

    //Configure
    host.ConfigureAppConfiguration(x => x.AddConfiguration(config));
    host.ConfigureServices(x => x.RegisterServices());
    host.ConfigureLoggers(config);
    host.ConfigureDiscord();

    //Use
    host.UseSerilog();
    host.UseConsoleLifetime();
    host.UseLavalink();

    //Start
    await host.RunConsoleAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Software crashed: {Exception}");
    Console.ReadLine();
    throw;
}
finally
{
    Log.CloseAndFlush();
    Environment.Exit(0);
}