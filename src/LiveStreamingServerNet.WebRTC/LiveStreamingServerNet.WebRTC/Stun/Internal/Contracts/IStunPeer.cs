using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts
{
    internal interface IStunPeer : IAsyncDisposable
    {
        Task<(StunMessage, UnknownAttributes?)> SendRequestAsync(StunMessage request, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        Task SendIndicationAsync(StunMessage indication, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        void FeedPacket(IDataBuffer buffer, IPEndPoint remoteEndPoint);
        void SetMessageHandler(IStunMessageHandler? handler);
    }
}
