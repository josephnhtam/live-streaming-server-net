using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpStreamManagerService
    {
        PublishingStreamResult StartPublishing(IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IList<IRtmpSubscribeStreamContext> subscribeStreamContexts);
        PublishingStreamResult StartDirectPublishing(IRtmpPublishStreamContext publishStreamContext, out IList<IRtmpSubscribeStreamContext> subscribeStreamContexts);
        bool StopPublishing(IRtmpPublishStreamContext publishStreamContext, out IList<IRtmpSubscribeStreamContext> subscribeStreamContexts);

        SubscribingStreamResult StartSubscribing(IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IRtmpPublishStreamContext? publishStreamContext);
        bool StopSubscribing(IRtmpSubscribeStreamContext subscribeStreamContext);

        IReadOnlyList<string> GetStreamPaths();
        IRtmpPublishStreamContext? GetPublishStreamContext(string streamPath);
        IReadOnlyList<IRtmpSubscribeStreamContext> GetSubscribeStreamContexts(string streamPath);
        bool IsStreamPublishing(string streamPath);
        bool IsStreamBeingSubscribed(string streamPath);
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
