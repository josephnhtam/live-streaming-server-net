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

        Task<(StunMessage, UnknownAttributes?)> SendStunRequestAsync(StunMessage request, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        ValueTask SendStunIndicationAsync(StunMessage indication, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        void SetStunMessageHandler(IStunMessageHandler? handler);

        event EventHandler<UdpTransportState> OnStateChanged;
        event EventHandler<IcePacketEventArgs> OnPacketReceived;
    }
}
