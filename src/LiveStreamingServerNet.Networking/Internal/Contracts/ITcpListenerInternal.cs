using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface ITcpListenerInternal : ITcpListener
    {
        void Start();
        void Stop();
        ValueTask<ITcpClientInternal> AcceptTcpClientAsync(CancellationToken cancellationToken);
    }
}
