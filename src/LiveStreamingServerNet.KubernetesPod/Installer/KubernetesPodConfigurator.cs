using LiveStreamingServerNet.KubernetesPod.Configurations;
using LiveStreamingServerNet.KubernetesPod.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.KubernetesPod.Installer
{
    public class KubernetesPodConfigurator : IKubernetesPodConfigurator
    {
        public IServiceCollection Services { get; }

        public KubernetesPodConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IKubernetesPodConfigurator Configure(Action<KubernetesPodConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);
            return this;
        }
    }
}
