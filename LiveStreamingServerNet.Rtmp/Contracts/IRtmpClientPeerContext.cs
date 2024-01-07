using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Handshakes;
using LiveStreamingServerNet.Rtmp.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    internal interface IRtmpClientPeerContext
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
        void DeleteStream();

        IRtmpPublishStreamContext? PublishStreamContext { get; }
        IRtmpPublishStreamContext CreatePublishStreamContext(string streamPath, IDictionary<string, string> streamArguments);

        IRtmpStreamSubscriptionContext? StreamSubscriptionContext { get; }
        IRtmpStreamSubscriptionContext CreateStreamSubscriptionContext(uint chunkStreamId, string streamPath, IDictionary<string, string> streamArguments);

        IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId);
    }

    internal interface IRtmpPublishStreamContext
    {
        uint StreamId { get; }
        string StreamPath { get; }
        IDictionary<string, string> StreamArguments { get; }
        IPublishStreamMetaData StreamMetaData { get; set; }
        byte[]? VideoSequenceHeader { get; set; }
        byte[]? AudioSequenceHeader { get; set; }
        IGroupOfPicturesCache GroupOfPicturesCache { get; }
    }

    internal interface IPublishStreamMetaData
    {
        uint VideoFrameRate { get; }
        uint VideoWidth { get; }
        uint VideoHeight { get; }

        uint AudioSampleRate { get; }
        uint AudioChannels { get; }
    }

    internal interface IRtmpStreamSubscriptionContext
    {
        uint StreamId { get; }
        uint ChunkStreamId { get; }
        string StreamPath { get; }
        IDictionary<string, string> StreamArguments { get; }

        bool IsPaused { get; set; }
        bool IsReceivingAudio { get; set; }
        bool IsReceivingVideo { get; set; }

        Task InitializationTask { get; }
        void CompleteInitialization();
    }

    internal interface IGroupOfPicturesCache
    {
        void Add(PicturesCache cache);
        void Clear(bool unclaim = true);
        IList<PicturesCache> Get(bool claim = true);
    }

    internal record struct PicturesCache
    {
        public MediaType Type { get; }
        public uint Timestamp { get; }
        public IRentedBuffer Payload { get; }
        public int PayloadSize { get; }

        public PicturesCache(MediaType type, uint timestamp, IRentedBuffer payload, int payloadSize)
        {
            Type = type;
            Timestamp = timestamp;
            Payload = payload;
            PayloadSize = payloadSize;
        }
    }
}
