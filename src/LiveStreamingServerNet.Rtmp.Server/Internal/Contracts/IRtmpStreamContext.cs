using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Contracts
{
    internal interface IRtmpStreamContext : IDisposable
    {
        uint StreamId { get; }
        uint CommandChunkStreamId { get; }

        IRtmpClientSessionContext ClientContext { get; }

        IRtmpPublishStreamContext? PublishContext { get; }
        IRtmpSubscribeStreamContext? SubscribeContext { get; }

        IRtmpPublishStreamContext CreatePublishContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        IRtmpSubscribeStreamContext CreateSubscribeContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments);

        void RemovePublishContext();
        void RemoveSubscribeContext();
    }

    internal interface IRtmpMediaStreamContext : IDisposable
    {
        string StreamPath { get; }
        IReadOnlyDictionary<string, string> StreamArguments { get; }

        uint VideoTimestamp { get; }
        uint AudioTimestamp { get; }
        bool UpdateTimestamp(uint timestamp, MediaType mediaType);
        void ResetTimestamps();
    }

    internal interface IRtmpPublishStreamContext : IRtmpMediaStreamContext
    {
        IRtmpStreamContext? StreamContext { get; }
        IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }

        byte[]? VideoSequenceHeader { get; set; }
        byte[]? AudioSequenceHeader { get; set; }

        bool GroupOfPicturesCacheActivated { get; set; }
        IGroupOfPicturesCache GroupOfPicturesCache { get; }

        DateTime StartTime { get; }

        uint TimestampOffset { get; }
        void SetTimestampOffset(uint timestampOffset);
    }

    internal interface IRtmpSubscribeStreamContext : IRtmpMediaStreamContext
    {
        IRtmpStreamContext StreamContext { get; }

        uint DataChunkStreamId { get; }
        uint AudioChunkStreamId { get; }
        uint VideoChunkStreamId { get; }

        bool IsReceivingAudio { get; set; }
        bool IsReceivingVideo { get; set; }

        void CompleteInitialization();
        Task UntilInitializationCompleteAsync(CancellationToken cancellationToken = default);
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
