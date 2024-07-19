using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpStreamInfo : IRtmpStreamInfo
    {
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }
        public IReadOnlyDictionary<string, object>? MetaData { get; }

        public DateTime StartTime { get; }
        public IClientControl Publisher { get; }
        public IReadOnlyList<IClientControl> Subscribers { get; }

        public RtmpStreamInfo(IClientControl publisher, IList<IClientControl> subscribers, IRtmpPublishStreamContext context)
        {
            StreamPath = context.StreamPath;
            StartTime = context.StartTime;
            StreamArguments = context.StreamArguments.ToDictionary(x => x.Key, x => x.Value);
            MetaData = context.StreamMetaData?.ToDictionary(x => x.Key, x => x.Value);

            Publisher = publisher;
            Subscribers = subscribers.ToList();
        }
    }

    internal class RtmpStream : IRtmpStream
    {
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }
        public IReadOnlyDictionary<string, object>? MetaData { get; }

        public DateTime StartTime { get; }
        public IClientControl Publisher { get; }
        public IReadOnlyList<IClientControl> Subscribers { get; }

        public RtmpStream(IClientControl publisher, IList<IClientControl> subscribers, IRtmpPublishStreamContext context)
        {
            StreamPath = context.StreamPath;
            StartTime = context.StartTime;
            StreamArguments = context.StreamArguments.ToDictionary(x => x.Key, x => x.Value);
            MetaData = context.StreamMetaData?.ToDictionary(x => x.Key, x => x.Value);

            Publisher = publisher;
            Subscribers = subscribers.ToList();
        }
    }
}
