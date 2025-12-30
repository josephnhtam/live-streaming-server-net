using System.Net;

namespace LiveStreamingServerNet.WebRTC.Udp.Internal.Contracts
{
    public interface IUdpTransport : IAsyncDisposable
    {
        IPEndPoint LocalEndPoint { get; }
        UdpTransportState State { get; }

        event EventHandler<UdpTransportState>? OnStateChanged;
        event EventHandler<UdpPacketEventArgs>? OnPacketReceived;

        bool Start();
        bool Close();
        bool SendPacket(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEndPoint);
    }
}
