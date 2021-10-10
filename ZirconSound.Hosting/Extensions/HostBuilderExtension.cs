using System;
using System.Linq;
using Lavalink4NET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ZirconSound.ApplicationCommands.Interactions;
using ZirconSound.Hosting.Helpers;
using ZirconSound.Hosting.Lavalink;

namespace ZirconSound.Hosting.Extensions
{
    public static class HostBuilderExtension
    {
        public static IHostBuilder UseInteractionService(this IHostBuilder builder, Action<HostBuilderContext, InteractionsServiceConfig> config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            builder.ConfigureServices((context, collection) =>
            {
                if (collection.Any(x => x.ServiceType == typeof(InteractionsService)))
                {
                    throw new InvalidOperationException("Cannot add more than one SlashCommandService to host");
                }

                collection.AddSingleton(typeof(LogAdapter<>));
                collection.Configure<InteractionsServiceConfig>(x => config(context, x));

                collection.AddSingleton(x => new InteractionsService(x.GetRequiredService<IOptions<InteractionsServiceConfig>>().Value));
                collection.AddHostedService<ServiceRegistrationHost>();
            });

            return builder;
        }

        public static IHostBuilder UseLavalink(this IHostBuilder builder, LavalinkNodeOptions config = null)
        {
            config ??= new LavalinkNodeOptions
            {
                RestUri = "http://localhost:2333/",
                WebSocketUri = "ws://localhost:2333/",
                Password = "youshallnotpass"
            };

            builder.ConfigureServices((_, collection) =>
            {
                collection.AddSingleton<IAudioService, HostingLavalinkNode>();
                collection.AddSingleton(config);
            });

            return builder;
        }
    }
}