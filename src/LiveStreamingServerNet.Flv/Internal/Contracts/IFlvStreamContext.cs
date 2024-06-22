using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvStreamContext : IDisposable
    {
        string StreamPath { get; }
        IReadOnlyDictionary<string, string> StreamArguments { get; }
        IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }
        byte[]? VideoSequenceHeader { get; set; }
        byte[]? AudioSequenceHeader { get; set; }
        IGroupOfPicturesCache GroupOfPicturesCache { get; }
        bool IsReady { get; }
    }

    internal interface IGroupOfPicturesCache : IDisposable
    {
        long Size { get; }
        void Add(PictureCacheInfo info, byte[] buffer, int start, int length);
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
