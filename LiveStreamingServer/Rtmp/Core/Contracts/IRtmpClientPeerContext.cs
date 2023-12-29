using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Rtmp.Core.Utilities;

namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IRtmpClientPeerContext
    {
        IClientPeerHandle Peer { get; }
        RtmpClientPeerState State { get; set; }
        HandshakeType HandshakeType { get; set; }

        uint InChunkSize { get; set; }
        uint OutChunkSize { get; set; }

        string AppName { get; set; }
        string PublishStreamPath { get; set; }
        IDictionary<string, string> PublishStreamArguments { get; set; }

        uint PublishStreamId { get; }
        uint NextPublishStreamId();

        IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId);
    }
}
