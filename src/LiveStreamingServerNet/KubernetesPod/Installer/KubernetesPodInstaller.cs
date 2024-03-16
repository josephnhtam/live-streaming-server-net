using LiveStreamingServerNet.KubernetesPod.Configurations;
using LiveStreamingServerNet.KubernetesPod.Internal.HostedServices;
using LiveStreamingServerNet.KubernetesPod.Internal.Services;
using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.KubernetesPod.Installer
{
    public static class KubernetesPodInstaller
    {
        public static IRtmpServerConfigurator AddKubernetesPodServices(
            this IRtmpServerConfigurator configurator, Action<KubernetesPodConfiguration>? configure = null)
        {
            var services = configurator.Services;

            services.AddHostedService<PodWatcherService>()
                    .AddSingleton<IKubernetesContext, KubernetesContext>()
                    .AddSingleton<IPodLifetimeManager, PodLifetimeManager>();

            configurator.AddConnectionEventHandler(svc => svc.GetRequiredService<IPodLifetimeManager>())
                        .AddStreamEventHandler(svc => svc.GetRequiredService<IPodLifetimeManager>());

            if (configure != null)
                services.Configure(configure);

            return configurator;
        }
    }
}
