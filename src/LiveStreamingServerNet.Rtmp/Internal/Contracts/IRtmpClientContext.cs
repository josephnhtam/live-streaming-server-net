using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Handshakes;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpClientContext
    {
        IClientHandle Client { get; }
        RtmpClientState State { get; set; }
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
        IDictionary<string, object>? StreamMetaData { get; set; }
        byte[]? VideoSequenceHeader { get; set; }
        byte[]? AudioSequenceHeader { get; set; }
        IGroupOfPicturesCache GroupOfPicturesCache { get; }
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

        void CompleteInitialization();
        Task UntilInitializationComplete();
    }

    internal interface IGroupOfPicturesCache
    {
        void Add(PicturesCache cache);
        void Clear(bool unclaim = true);
        IList<PicturesCache> Get(bool claim = true);
    }

    internal readonly record struct PicturesCache
    {
        public MediaType Type { get; }
        public uint Timestamp { get; }
        public IRentedBuffer Payload { get; }

        public PicturesCache(MediaType type, uint timestamp, IRentedBuffer payload)
        {
            Type = type;
            Timestamp = timestamp;
            Payload = payload;
        }
    }
}
