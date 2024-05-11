using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface INetBufferSender : IAsyncDisposable
    {
        void Start(INetworkStreamWriter networkStream, CancellationToken cancellationToken);
        void Send(INetBuffer netBuffer, Action<bool>? callback);
        void Send(IRentedBuffer rentedBuffer, Action<bool>? callback);
        void Send(Action<INetBuffer> writer, Action<bool>? callback);
        ValueTask SendAsync(INetBuffer netBuffer);
        ValueTask SendAsync(IRentedBuffer rentedBuffer);
        ValueTask SendAsync(Action<INetBuffer> writer);
    }
}
