using LiveStreamingServerNet.Networking.Server.Contracts;

namespace LiveStreamingServerNet
{
    public interface ILiveStreamingServer : IServer, IDisposable, IAsyncDisposable { }
}
