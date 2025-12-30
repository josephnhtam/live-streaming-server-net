using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts
{
    public interface IStunSender
    {
        ValueTask SendAsync(IDataBuffer buffer, IPEndPoint remoteEndpoint, CancellationToken cancellation);
    }
}
