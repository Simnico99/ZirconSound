namespace ZirconSound.Hosting.Extensions;

public static class HostBuilderExtension
{
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
            collection.AddSingleton<IAudioService, LavalinkNode>();
            collection.AddSingleton(config);
        });

        return builder;
    }
}
