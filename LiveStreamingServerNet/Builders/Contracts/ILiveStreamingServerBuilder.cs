using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Builders.Contracts
{
    public interface ILiveStreamingServerBuilder
    {
        ILiveStreamingServerBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging);
        IServer Build();
    }
}