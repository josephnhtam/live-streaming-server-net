using LiveStreamingServerNet.Newtorking.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Contracts
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
        void Send(INetBuffer netBuffer, Action? callback = null);
        void Send(Action<INetBuffer> writer, Action? callback = null);
        Task SendAsync(INetBuffer netBuffer);
        Task SendAsync(Action<INetBuffer> writer);
    }
}
