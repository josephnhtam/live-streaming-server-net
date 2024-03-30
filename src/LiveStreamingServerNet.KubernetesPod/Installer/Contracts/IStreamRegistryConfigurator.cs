using LiveStreamingServerNet.KubernetesPod.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.KubernetesPod.Installer.Contracts
{
    public interface IStreamRegistryConfigurator
    {
        IServiceCollection Services { get; }
        IStreamRegistryConfigurator Configure(Action<StreamRegistryConfiguration> configure);
    }
}
