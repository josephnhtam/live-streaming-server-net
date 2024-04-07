using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface INetBufferSender : IAsyncDisposable
    {
        void Start(Stream networkStream, CancellationToken cancellationToken);
        void Send(INetBuffer netBuffer, Action<bool>? callback);
        void Send(Action<INetBuffer> writer, Action<bool>? callback);
        Task SendAsync(INetBuffer netBuffer);
        Task SendAsync(Action<INetBuffer> writer);
    }
}
