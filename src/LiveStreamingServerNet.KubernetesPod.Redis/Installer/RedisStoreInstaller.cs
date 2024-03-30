using LiveStreamingServerNet.KubernetesPod.Configurations;
using LiveStreamingServerNet.KubernetesPod.Installer.Contracts;
using LiveStreamingServerNet.KubernetesPod.Redis.Configurations;
using LiveStreamingServerNet.KubernetesPod.Redis.Internal.Services;
using LiveStreamingServerNet.KubernetesPod.Redis.Internal.Services.Contracts;
using LiveStreamingServerNet.KubernetesPod.StreamRegistration.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace LiveStreamingServerNet.KubernetesPod.Redis.Installer
{
    public static class RedisStoreInstaller
    {
        public static IStreamRegistryConfigurator UseRedisStore(
            this IStreamRegistryConfigurator configurator,
            IConnectionMultiplexer redisConnection,
            Action<RedisStoreConfiguration>? configure = null)
        {
            var services = configurator.Services;

            services.AddSingleton<IStreamKeyProvider, StreamKeyProvider>();

            services.TryAddSingleton<IStreamStore>(
                svc => new StreamStore(
                    redisConnection.GetDatabase(),
                    svc.GetRequiredService<IStreamKeyProvider>(),
                    svc.GetRequiredService<ILogger<StreamStore>>(),
                    svc.GetRequiredService<IOptions<StreamRegistryConfiguration>>()
                )
            );

            if (configure != null)
                services.Configure(configure);

            return configurator;
        }
    }
}
