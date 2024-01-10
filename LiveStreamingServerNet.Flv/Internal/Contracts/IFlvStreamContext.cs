using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvStreamContext
    {
        string StreamPath { get; }
        IDictionary<string, string> StreamArguments { get; }
        IDictionary<string, object>? StreamMetaData { get; set; }
        byte[]? VideoSequenceHeader { get; set; }
        byte[]? AudioSequenceHeader { get; set; }
        IGroupOfPicturesCache GroupOfPicturesCache { get; }
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
