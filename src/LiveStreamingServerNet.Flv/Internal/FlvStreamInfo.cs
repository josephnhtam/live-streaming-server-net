using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvStreamInfo: IFlvStreamInfo
    {
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }
        public IReadOnlyDictionary<string, object>? MetaData { get; }
        public IReadOnlyList<IFlvClientHandle> Subscribers { get; }

        public FlvStreamInfo(IFlvStreamContext context, IList<IFlvClientHandle> subscribers)
        {
            StreamPath = context.StreamPath;
            StreamArguments = context.StreamArguments.ToDictionary(x => x.Key, x => x.Value);
            MetaData = context.StreamMetaData?.ToDictionary(x => x.Key, x => x.Value);
            Subscribers = subscribers.ToList();
        }
    }
}
