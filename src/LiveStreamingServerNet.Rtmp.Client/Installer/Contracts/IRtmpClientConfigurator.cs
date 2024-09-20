using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Installer.Contracts
{
    public interface IRtmpClientConfigurator
    {
        IServiceCollection Services { get; }
    }
}