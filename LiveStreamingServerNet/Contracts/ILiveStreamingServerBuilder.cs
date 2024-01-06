using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Configurations;
using LiveStreamingServerNet.Rtmp.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Contracts
{
    public interface ILiveStreamingServerBuilder
    {
        IServiceCollection Services { get; }
        ILiveStreamingServerBuilder ConfigureLogging(Action<ILoggingBuilder> configure);
        ILiveStreamingServerBuilder ConfigureRtmpServer(Action<RtmpServerConfiguration> configure);
        ILiveStreamingServerBuilder ConfigureMediaMessage(Action<MediaMessageConfiguration> configure);
        ILiveStreamingServerBuilder ConfigureNetBufferPool(Action<NetBufferPoolConfiguration> configure);
        IServer Build();
    }
}