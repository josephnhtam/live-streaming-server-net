using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Server.Contracts;

namespace LiveStreamingServerNet.Networking.Server.Internal.Contracts
{
    internal interface ITcpListenerInternal : ITcpListener
    {
        void Start();
        void Stop();
        ValueTask<ITcpClientInternal> AcceptTcpClientAsync(CancellationToken cancellationToken);
    }
}
