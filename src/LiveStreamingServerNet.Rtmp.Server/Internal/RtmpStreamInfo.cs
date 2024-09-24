using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal class RtmpStreamInfo : IRtmpStreamInfo
    {
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }
        public IReadOnlyDictionary<string, object>? MetaData { get; }

        public DateTime StartTime { get; }
        public ISessionControl Publisher { get; }
        public IReadOnlyList<ISessionControl> Subscribers { get; }

        public RtmpStreamInfo(ISessionControl publisher, IList<ISessionControl> subscribers, IRtmpPublishStreamContext context)
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
