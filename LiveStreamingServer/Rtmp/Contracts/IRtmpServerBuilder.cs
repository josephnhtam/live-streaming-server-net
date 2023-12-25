using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Contracts
{
    public interface IRtmpServerBuilder
    {
        IRtmpServerBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging);
        RtmpServer Build();
    }
}