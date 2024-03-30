using LiveStreamingServerNet.KubernetesPod.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.KubernetesPod.Installer.Contracts
{
    public interface IKubernetesPodConfigurator
    {
        IServiceCollection Services { get; }
        IKubernetesPodConfigurator Configure(Action<KubernetesPodConfiguration>? configure);
        IKubernetesPodConfigurator AddStreamRegistry(Action<IStreamRegistryConfigurator> configure);
    }
}
