using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    public interface IUdpConnection
    {
        IPEndPoint LocalEndPoint { get; }
        bool SendPacket(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEndPoint);
        event EventHandler<UdpPacketEventArgs?>? OnPacketReceived;

        void Start();
    }
}
