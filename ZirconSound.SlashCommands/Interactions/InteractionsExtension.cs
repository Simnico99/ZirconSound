using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using ZirconSound.ApplicationCommands.Helpers;

namespace ZirconSound.ApplicationCommands.Interactions
{
    public static class InteractionsExtension
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
                collection.AddHostedService<InteractionsServiceRegistrationHost>();
            });

            return builder;
        }
    }
}