using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet
{
    public interface ILiveStreamingServer : IServer, IDisposable, IAsyncDisposable { }
}
