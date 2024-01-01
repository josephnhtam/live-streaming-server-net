using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Handshakes;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpClientPeerContext
    {
        IClientPeerHandle Peer { get; }
        RtmpClientPeerState State { get; set; }
        HandshakeType HandshakeType { get; set; }

        uint InChunkSize { get; set; }
        uint OutChunkSize { get; set; }

        uint InWindowAcknowledgementSize { get; set; }
        uint OutWindowAcknowledgementSize { get; set; }

        string AppName { get; set; }

        IRtmpPublishStreamContext? PublishStreamContext { get; }
        IRtmpPublishStreamContext CreateNewPublishStream();

        IRtmpStreamSubscriptionContext? StreamSubscriptionContext { get; }
        IRtmpStreamSubscriptionContext CreateStreamSubscriptionContext(uint chunkStreamId, string streamName, IDictionary<string, string> streamArguments);

        IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId);
    }

    public interface IRtmpPublishStreamContext
    {
        uint Id { get; }
        string StreamPath { get; set; }
        IDictionary<string, string> StreamArguments { get; set; }
        IPublishStreamMetaData StreamMetaData { get; set; }
        byte[]? VideoSequenceHeader { get; set; }
        byte[]? AudioSequenceHeader { get; set; }
    }

    public interface IPublishStreamMetaData
    {
        uint VideoFrameRate { get; }
        uint VideoWidth { get; }
        uint VideoHeight { get; }

        uint AudioSampleRate { get; }
        uint AudioChannels { get; }
    }

    public interface IRtmpStreamSubscriptionContext
    {
        uint ChunkStreamId { get; }
        string StreamPath { get; }
        IDictionary<string, string> StreamArguments { get; }
    }
}
