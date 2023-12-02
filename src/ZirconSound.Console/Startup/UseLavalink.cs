using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking.Extensions;
using Microsoft.Extensions.Hosting;

namespace ZirconSound.Console.Startup;
public static partial class HostBuilderExtension
{
    public static IHostBuilder UseLavalink(this IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddLavalink();
            services.ConfigureLavalink(config =>
            {
                config.BaseAddress = new Uri("http://localhost:2333/");
                config.WebSocketUri = new Uri("ws://localhost:2333/v4/websocket");
                config.Passphrase = "youshallnotpass";
                config.ReadyTimeout = TimeSpan.FromMinutes(1);
            });
            services.AddInactivityTracking();
            services.ConfigureInactivityTracking(config => config.DefaultTimeout = TimeSpan.FromMinutes(1));
        });

        return builder;
    }
}
