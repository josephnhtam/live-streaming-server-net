using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Builders.Contracts
{
    public interface ILiveStreamingServerBuilder
    {
        ILiveStreamingServerBuilder ConfigureLogging(Action<ILoggingBuilder> configure);
        ILiveStreamingServerBuilder ConfigureMediaMessage(Action<MediaMessageConfiguration> configure);
        IServer Build();
    }
}