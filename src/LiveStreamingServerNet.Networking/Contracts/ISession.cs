using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Net;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface ISessionInfo
    {
        uint Id { get; }
        bool IsConnected { get; }
        DateTime StartTime { get; }
        EndPoint LocalEndPoint { get; }
        EndPoint RemoteEndPoint { get; }
    }

    public interface ISessionControl : ISessionInfo
    {
        void Disconnect();
        Task DisconnectAsync(CancellationToken cancellation = default);
    }

    public interface ISessionHandle : ISessionControl
    {
        void Send(IDataBuffer dataBuffer, Action<bool>? callback = null);
        void Send(IRentedBuffer rentedBuffer, Action<bool>? callback = null);
        void Send(Action<IDataBuffer> writer, Action<bool>? callback = null);
        ValueTask SendAsync(IDataBuffer dataBuffer);
        ValueTask SendAsync(IRentedBuffer rentedBuffer);
        ValueTask SendAsync(Action<IDataBuffer> writer);
    }
}
