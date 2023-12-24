using LiveStreamingServer.Newtorking.Contracts;

namespace LiveStreamingServer.Networking.Contracts
{
    public interface IClientPeer : IAsyncDisposable
    {
        uint PeerId { get; }
        bool IsConnected { get; }
        void Disconnect();
        void Send(INetBuffer netBuffer);
        void Send(Action<INetBuffer> callback);
        Task SendAsync(Func<INetBuffer, Task> callback);
    }
}
