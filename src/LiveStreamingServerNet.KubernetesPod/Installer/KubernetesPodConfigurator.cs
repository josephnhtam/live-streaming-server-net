using LiveStreamingServerNet.KubernetesPod.Configurations;
using LiveStreamingServerNet.KubernetesPod.Installer.Contracts;
using LiveStreamingServerNet.KubernetesPod.Internal.Services;
using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.KubernetesPod.Installer
{
    internal class KubernetesPodConfigurator : IKubernetesPodConfigurator
    {
        private readonly IRtmpServerConfigurator _rtmpServerConfigurator;

        public IServiceCollection Services { get; }

        public KubernetesPodConfigurator(IRtmpServerConfigurator rtmpServerConfigurator, IServiceCollection services)
        {
            _rtmpServerConfigurator = rtmpServerConfigurator;
            Services = services;
        }

        public IKubernetesPodConfigurator Configure(Action<KubernetesPodConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);
            return this;
        }

        public IKubernetesPodConfigurator AddStreamRegistry(Action<IStreamRegistryConfigurator> configure)
        {
            Services.AddSingleton<IStreamRegistry, StreamRegistry>();

            _rtmpServerConfigurator
                .AddAuthorizationHandler<StreamRegistrationHandler>()
                .AddStreamEventHandler<StreamUnregistrationHandler>();

            var configurator = new StreamRegistryConfigurator(Services);
            configure(configurator);

            return this;
        }
    }
}
