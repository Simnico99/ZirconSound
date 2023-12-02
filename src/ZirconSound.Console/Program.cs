using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using ZirconSound.Console.Startup;

try
{
    //Create
    var builder = Host.CreateDefaultBuilder(args);
    var config = new ConfigurationBuilder().RegisterConfigurations().Build();

    //Configure
    builder.ConfigureAppConfiguration(x => x.AddConfiguration(config));
    builder.ConfigureServices(x => x.RegisterServices());
    builder.ConfigureLoggers(config);
    builder.ConfigureDiscord();

    //Use
    builder.UseSerilog();
    builder.UseConsoleLifetime();
    builder.UseLavalink();

    //Start
    await builder.RunConsoleAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Software crashed: {Exception}");
    Console.ReadLine();
    Log.CloseAndFlush();
    throw;
}
finally
{
    Log.CloseAndFlush();
    Environment.Exit(0);
}