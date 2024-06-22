using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Handshakes;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpClientContext : IAsyncDisposable
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

        uint VideoTimestamp { get; }
        uint AudioTimestamp { get; }

        uint CreateNewStream();
        void DeleteStream();

        IRtmpPublishStreamContext? PublishStreamContext { get; }
        IRtmpPublishStreamContext CreatePublishStreamContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments);

        IRtmpStreamSubscriptionContext? StreamSubscriptionContext { get; }
        IRtmpStreamSubscriptionContext CreateStreamSubscriptionContext(uint chunkStreamId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);

        IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId);

        bool UpdateTimestamp(uint timestamp, MediaType mediaType);
    }

    internal interface IRtmpPublishStreamContext : IAsyncDisposable
    {
        uint StreamId { get; }
        string StreamPath { get; }
        IReadOnlyDictionary<string, string> StreamArguments { get; }
        IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }
        byte[]? VideoSequenceHeader { get; set; }
        byte[]? AudioSequenceHeader { get; set; }
        bool GroupOfPicturesCacheActivated { get; set; }
        IGroupOfPicturesCache GroupOfPicturesCache { get; }
    }

    internal interface IRtmpStreamSubscriptionContext : IAsyncDisposable
    {
        uint StreamId { get; }
        uint ChunkStreamId { get; }
        string StreamPath { get; }
        IReadOnlyDictionary<string, string> StreamArguments { get; }

        bool IsReceivingAudio { get; set; }
        bool IsReceivingVideo { get; set; }

        void CompleteInitialization();
        Task UntilInitializationComplete();
    }

    internal interface IGroupOfPicturesCache : IDisposable
    {
        long Size { get; }
        void Add(PictureCacheInfo info, IDataBuffer buffer);
        void Clear();
        IList<PictureCache> Get(int initialClaim = 1);
    }

    internal readonly record struct PictureCacheInfo
    {
        public MediaType Type { get; }
        public uint Timestamp { get; }

        public PictureCacheInfo(MediaType type, uint timestamp)
        {
            Type = type;
            Timestamp = timestamp;
        }
    }

    internal readonly record struct PictureCache
    {
        public MediaType Type { get; }
        public uint Timestamp { get; }
        public IRentedBuffer Payload { get; }

        public PictureCache(MediaType type, uint timestamp, IRentedBuffer payload)
        {
            Type = type;
            Timestamp = timestamp;
            Payload = payload;
        }
    }
}
