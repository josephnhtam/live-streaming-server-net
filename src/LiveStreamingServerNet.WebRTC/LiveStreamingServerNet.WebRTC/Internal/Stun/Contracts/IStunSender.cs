using System.Net;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Contracts
{
    public interface IStunSender
    {
        ValueTask SendAsync(ReadOnlySpan<byte> buffer, IPEndPoint remoteEndpoint, CancellationToken cancellation);
    }
}
