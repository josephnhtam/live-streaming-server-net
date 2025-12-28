using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Udp.Internal
{
    public record struct UdpPacketEventArgs(IRentedBuffer RentedBuffer, IPEndPoint RemoteEndPoint);
}
