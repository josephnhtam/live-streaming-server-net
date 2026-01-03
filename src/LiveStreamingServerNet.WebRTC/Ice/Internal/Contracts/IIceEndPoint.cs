using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Udp.Internal;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IIceEndPoint : IAsyncDisposable
    {
        UdpTransportState State { get; }

        bool Start();
        bool Close();

        Task<StunResponse> SendStunRequestAsync(StunMessage request, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        ValueTask SendStunIndicationAsync(StunMessage indication, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        bool SendPacket(ReadOnlyMemory<byte> packet, IPEndPoint remoteEndPoint);
        void SetStunMessageHandler(IStunMessageHandler? handler);

        event EventHandler<UdpTransportState> OnStateChanged;
        event EventHandler<IcePacketEventArgs> OnPacketReceived;
    }
}
