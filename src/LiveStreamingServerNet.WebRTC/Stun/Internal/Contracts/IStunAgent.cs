using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts
{
    internal interface IStunAgent : IAsyncDisposable
    {
        Task<StunResponse> SendRequestAsync(StunMessage request, IPEndPoint remoteEndPoint, StunRetransmissionOptions? retransmissionOptions = null, CancellationToken cancellation = default);
        ValueTask SendIndicationAsync(StunMessage indication, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        void FeedPacket(IDataBufferReader buffer, IPEndPoint remoteEndPoint, object? state = null);
        void SetMessageHandler(IStunMessageHandler? handler);
    }
}
