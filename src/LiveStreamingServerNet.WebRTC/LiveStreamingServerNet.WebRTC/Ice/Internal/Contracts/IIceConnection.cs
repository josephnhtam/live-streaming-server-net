using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    public interface IIceConnection
    {
        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }

        ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellation);
        ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellation);
    }
}
