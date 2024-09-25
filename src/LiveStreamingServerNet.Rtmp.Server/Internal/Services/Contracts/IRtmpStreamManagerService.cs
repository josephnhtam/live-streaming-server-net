using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpStreamManagerService
    {
        PublishingStreamResult StartPublishing(IRtmpStream stream, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IList<IRtmpSubscribeStreamContext> subscribeStreamContexts);
        bool StopPublishing(IRtmpPublishStreamContext publishStreamContext, out IList<IRtmpSubscribeStreamContext> subscribeStreamContexts);

        SubscribingStreamResult StartSubscribing(IRtmpStream stream, uint chunkStreamId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        bool StopSubscribing(IRtmpSubscribeStreamContext subscribeStreamContext);

        IReadOnlyList<string> GetStreamPaths();
        IRtmpPublishStreamContext? GetPublishStreamContext(string streamPath);
        IReadOnlyList<IRtmpSubscribeStreamContext> GetSubscribeStreamContexts(string streamPath);
        bool IsStreamPublishing(string streamPath);
    }

    internal enum PublishingStreamResult
    {
        Succeeded,
        AlreadyExists,
        AlreadyPublishing,
        AlreadySubscribing
    }

    internal enum SubscribingStreamResult
    {
        Succeeded,
        AlreadyPublishing,
        AlreadySubscribing,
    }
}
