using LiveStreamingServerNet.Utilities.Contracts;
using System.Net;

namespace LiveStreamingServerNet.Networking.Contracts
{
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
        void Send(IRentedBuffer rentedBuffer, Action<bool>? callback = null);
        void Send(Action<INetBuffer> writer, Action<bool>? callback = null);
        ValueTask SendAsync(INetBuffer netBuffer);
        ValueTask SendAsync(IRentedBuffer rentedBuffer);
        ValueTask SendAsync(Action<INetBuffer> writer);
    }
}
