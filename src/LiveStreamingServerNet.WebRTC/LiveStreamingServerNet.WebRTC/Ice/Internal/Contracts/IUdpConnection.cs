using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    public interface IUdpConnection
    {
        IPEndPoint LocalEndPoint { get; }
        UdpConnectionState State { get; }

        event EventHandler<UdpConnectionState>? OnStateChanged;
        event EventHandler<UdpPacketEventArgs>? OnPacketReceived;

        bool Start();
        bool Close();
        bool SendPacket(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEndPoint);
    }
}
