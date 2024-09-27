using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Contracts
{
    internal interface IRtmpStream
    {
        uint Id { get; }
        uint VideoTimestamp { get; }
        uint AudioTimestamp { get; }

        IRtmpClientSessionContext ClientContext { get; }

        IRtmpPublishStreamContext? PublishContext { get; }
        IRtmpSubscribeStreamContext? SubscribeContext { get; }

        IRtmpPublishStreamContext CreatePublishContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        IRtmpSubscribeStreamContext CreateSubscribeContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments);

        void RemovePublishContext();
        void RemoveSubscribeContext();

        bool UpdateTimestamp(uint timestamp, MediaType mediaType);
        void Delete();
    }

    internal interface IRtmpPublishStreamContext : IDisposable
    {
        IRtmpStream Stream { get; }
        string StreamPath { get; }
        IReadOnlyDictionary<string, string> StreamArguments { get; }
        IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }

        byte[]? VideoSequenceHeader { get; set; }
        byte[]? AudioSequenceHeader { get; set; }

        bool GroupOfPicturesCacheActivated { get; set; }
        IGroupOfPicturesCache GroupOfPicturesCache { get; }

        DateTime StartTime { get; }
    }

    internal interface IRtmpSubscribeStreamContext : IDisposable
    {
        IRtmpStream Stream { get; }
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
