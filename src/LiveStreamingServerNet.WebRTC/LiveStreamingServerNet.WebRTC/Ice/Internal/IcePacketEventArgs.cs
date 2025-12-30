using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    public record struct IcePacketEventArgs(IRentedBuffer RentedBuffer, IPEndPoint RemoteEndPoint);
}
