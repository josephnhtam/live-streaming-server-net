using System.Net;

namespace LiveStreamingServerNet.Networking.Contracts
{
    internal interface IClient : IAsyncDisposable, IClientHandle
    {
        Task RunAsync(IClientHandler handler, ServerEndPoint serverEndPoint, CancellationToken stoppingToken);
    }

    public interface IClientInfo
    {
        uint ClientId { get; }
        bool IsConnected { get; }
        EndPoint LocalEndPoint { get; }
        EndPoint RemoteEndPoint { get; }
    }

    public interface IClientControl : IClientInfo
    {
        void Disconnect();
        Task DisconnectAsync(CancellationToken cancellation = default);
    }

    public interface IClientHandle : IClientControl
    {
        void Send(INetBuffer netBuffer, Action<bool>? callback = null);
        void Send(Action<INetBuffer> writer, Action<bool>? callback = null);
        Task SendAsync(INetBuffer netBuffer);
        Task SendAsync(Action<INetBuffer> writer);
    }
}
