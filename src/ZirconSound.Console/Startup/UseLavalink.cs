using Lavalink4NET;
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
    public static IHostBuilder UseLavalink(this IHostBuilder builder, LavalinkNodeOptions? config = null)
    {
        config ??= new LavalinkNodeOptions
        {
            RestUri = "http://localhost:2333/",
            WebSocketUri = "ws://localhost:2333/",
            Password = "youshallnotpass"
        };

        builder.ConfigureServices((_, collection) =>
        {
            collection.AddSingleton<IAudioService, LavalinkNode>();
            collection.AddSingleton(config);
        });

        return builder;
    }
}
