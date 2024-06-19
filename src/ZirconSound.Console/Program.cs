using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using ZirconSound.Console;

try
{
    //Create
    var builder = Host.CreateDefaultBuilder(args);
    var config = new ConfigurationBuilder().RegisterConfigurations().Build();

    //Add
    builder.AddServices();
    builder.AddDiscordServices(config);

    //Configure
    builder.ConfigureAppConfiguration(x => x.AddConfiguration(config));
    builder.ConfigureLoggers(config);


    //Use
    builder.UseSerilog();
    builder.UseConsoleLifetime();
    builder.UseLavalink(config);

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