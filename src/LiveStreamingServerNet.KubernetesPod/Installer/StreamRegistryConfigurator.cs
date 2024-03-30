using LiveStreamingServerNet.KubernetesPod.Configurations;
using LiveStreamingServerNet.KubernetesPod.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.KubernetesPod.Installer
{
    internal class StreamRegistryConfigurator : IStreamRegistryConfigurator
    {
        public IServiceCollection Services { get; }

        public StreamRegistryConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IStreamRegistryConfigurator Configure(Action<StreamRegistryConfiguration> configure)
        {
            Services.Configure(configure);
            return this;
        }
    }
}
