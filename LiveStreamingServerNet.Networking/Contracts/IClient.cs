using LiveStreamingServerNet.Newtorking.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IClient : IAsyncDisposable, IClientHandle
    {
        void Initialize(uint clientId, TcpClient tcpClient);
        Task RunAsync(IClientHandler handler, CancellationToken stoppingToken);
    }

    public interface IClientHandle
    {
        uint ClientId { get; }
        bool IsConnected { get; }
        void Disconnect();
        void Send(INetBuffer netBuffer, Action? callback = null);
        void Send(Action<INetBuffer> writer, Action? callback = null);
        Task SendAsync(INetBuffer netBuffer);
        Task SendAsync(Action<INetBuffer> writer);
    }
}
