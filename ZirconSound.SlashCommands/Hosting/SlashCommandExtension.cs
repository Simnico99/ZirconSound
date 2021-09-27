using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ZirconSound.SlashCommands.Hosting
{
    public static class SlashCommandExtension
    {
        public static IHostBuilder UseSlashCommandService(this IHostBuilder builder, Action<HostBuilderContext, SlashCommandServiceConfig> config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            builder.ConfigureServices((context, collection) =>
            {
                if (collection.Any(x => x.ServiceType == typeof(SlashCommandService)))
                {
                    throw new InvalidOperationException("Cannot add more than one SlashCommandService to host");
                }

                collection.AddSingleton(typeof(LogAdapter<>));
                collection.Configure<SlashCommandServiceConfig>(x => config(context, x));

                collection.AddSingleton(x => new SlashCommandService(x.GetRequiredService<IOptions<SlashCommandServiceConfig>>().Value));
                collection.AddHostedService<SlashCommandServiceRegistrationHost>();
            });

            return builder;
        }
    }
}