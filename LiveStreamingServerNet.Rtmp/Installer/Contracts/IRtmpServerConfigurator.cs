using LiveStreamingServerNet.Rtmp.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Installer.Contracts
{
    public interface IRtmpServerConfigurator
    {
        IServiceCollection Services { get; }
        IRtmpServerConfigurator ConfigureRtmpServer(Action<RtmpServerConfiguration>? configure);
        IRtmpServerConfigurator ConfigureMediaMessage(Action<MediaMessageConfiguration>? configure);
    }
}
