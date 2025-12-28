using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    public class IceConnection : IIceConnection
    {
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }
    }
}
