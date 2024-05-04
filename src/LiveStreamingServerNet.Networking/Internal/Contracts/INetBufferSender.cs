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
        Task SendAsync(INetBuffer netBuffer);
        Task SendAsync(IRentedBuffer rentedBuffer);
        Task SendAsync(Action<INetBuffer> writer);
    }
}
