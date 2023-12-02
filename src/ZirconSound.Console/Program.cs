using Lavalink4NET.InactivityTracking;
using Lavalink4NET.InactivityTracking.Extensions;
using Lavalink4NET.InactivityTracking.Trackers.Idle;
using Lavalink4NET.InactivityTracking.Trackers.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    host.ConfigureServices(services =>
    {
        //services.AddSingleton<IInactivityTracker, InactiveUserTracker>();
        services.UseLavalink();
        services.AddInactivityTracking();
        services.ConfigureInactivityTracking(config => config.DefaultTimeout = TimeSpan.FromMinutes(1));

    });

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