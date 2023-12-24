using LiveStreamingServer.Newtorking.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServer.Networking.Contracts
{
    public interface IClientPeer : IAsyncDisposable, IClientPeerHandle
    {
        void Initialize(uint peerId, TcpClient tcpClient);
        Task RunAsync(IClientPeerHandler handler, CancellationToken stoppingToken);
    }

    public interface IClientPeerHandle
    {
        uint PeerId { get; }
        bool IsConnected { get; }
        void Disconnect();
        void Send(INetBuffer netBuffer);
        void Send(Action<INetBuffer> callback);
        Task SendAsync(Func<INetBuffer, Task> callback);
    }
}
