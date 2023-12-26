using LiveStreamingServer.Rtmp.Core.Utilities;

namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IRtmpClientPeerContext
    {
        RtmpClientPeerState State { get; set; }
        HandshakeType HandshakeType { get; set; }
        IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId);
    }
}
