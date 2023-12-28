using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Rtmp.Core.Utilities;

namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IRtmpClientPeerContext
    {
        IClientPeerHandle Peer { get; }
        RtmpClientPeerState State { get; set; }
        HandshakeType HandshakeType { get; set; }
        IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId);
        uint InChunkSize { get; set; }
        uint OutChunkSize { get; set; }
    }
}
