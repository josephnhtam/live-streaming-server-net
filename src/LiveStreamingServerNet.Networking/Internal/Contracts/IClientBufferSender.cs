using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface IClientBufferSender : IAsyncDisposable
    {
        void Start(INetworkStreamWriter networkStream, CancellationToken cancellationToken);
        void Send(IDataBuffer dataBuffer, Action<bool>? callback);
        void Send(IRentedBuffer rentedBuffer, Action<bool>? callback);
        void Send(Action<IDataBuffer> writer, Action<bool>? callback);
        ValueTask SendAsync(IDataBuffer dataBuffer);
        ValueTask SendAsync(IRentedBuffer rentedBuffer);
        ValueTask SendAsync(Action<IDataBuffer> writer);
    }
}
