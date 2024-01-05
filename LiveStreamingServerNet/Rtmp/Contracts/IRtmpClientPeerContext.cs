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

        uint SequenceNumber { get; set; }
        uint LastAcknowledgedSequenceNumber { get; set; }

        string AppName { get; set; }
        uint? StreamId { get; }

        uint CreateNewStream();

        IRtmpPublishStreamContext? PublishStreamContext { get; }
        IRtmpPublishStreamContext CreatePublishStreamContext(string streamPath, IDictionary<string, string> streamArguments);

        IRtmpStreamSubscriptionContext? StreamSubscriptionContext { get; }
        IRtmpStreamSubscriptionContext CreateStreamSubscriptionContext(uint chunkStreamId, string streamPath, IDictionary<string, string> streamArguments);

        IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId);
    }

    public interface IRtmpPublishStreamContext
    {
        uint StreamId { get; }
        string StreamPath { get; }
        IDictionary<string, string> StreamArguments { get; }
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
        uint StreamId { get; }
        uint ChunkStreamId { get; }
        string StreamPath { get; }
        IDictionary<string, string> StreamArguments { get; }
    }
}
