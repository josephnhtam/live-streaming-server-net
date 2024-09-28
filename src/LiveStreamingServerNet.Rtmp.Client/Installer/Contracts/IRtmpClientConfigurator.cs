using LiveStreamingServerNet.Rtmp.Client.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Installer.Contracts
{
    public interface IRtmpClientConfigurator
    {
        IServiceCollection Services { get; }

        IRtmpClientConfigurator Configure(Action<RtmpClientConfiguration>? configure);
    }
}