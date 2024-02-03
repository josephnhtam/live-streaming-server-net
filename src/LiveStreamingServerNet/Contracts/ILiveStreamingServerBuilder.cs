using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Contracts
{
    public interface ILiveStreamingServerBuilder
    {
        IServiceCollection Services { get; }
        ILiveStreamingServerBuilder ConfigureLogging(Action<ILoggingBuilder> configure);
        ILiveStreamingServerBuilder ConfigureRtmpServer(Action<IRtmpServerConfigurator> configure);
        ILiveStreamingServerBuilder ConfigureServer(Action<IServerConfigurator> configure);
        ILiveStreamingServer Build();
    }
}