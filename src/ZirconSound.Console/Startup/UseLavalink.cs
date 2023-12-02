using Lavalink4NET;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZirconSound.Console.Startup;
public static partial class HostBuilderExtension
{
    public static IServiceCollection UseLavalink(this IServiceCollection services)
    {
        services.AddLavalink();
        services.ConfigureLavalink(config =>
        {
            config.BaseAddress = new Uri("http://localhost:2333/");
            config.WebSocketUri = new Uri("ws://localhost:2333/v4/websocket");
            config.Passphrase = "youshallnotpass";
            config.ReadyTimeout = TimeSpan.FromMinutes(1);
        });

        return services;
    }
}
