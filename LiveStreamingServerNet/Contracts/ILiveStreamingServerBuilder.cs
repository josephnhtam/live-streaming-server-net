using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Contracts
{
    public interface ILiveStreamingServerBuilder
    {
        ILiveStreamingServerBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging);
        IServer Build();
    }
}