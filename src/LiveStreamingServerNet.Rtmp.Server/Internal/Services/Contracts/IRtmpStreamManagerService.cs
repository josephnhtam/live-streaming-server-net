using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpStreamManagerService
    {
        ValueTask<(PublishingStreamResult Result, IList<IRtmpSubscribeStreamContext> SubscribeStreamContexts)> StartPublishingAsync(IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask<(PublishingStreamResult Result, IList<IRtmpSubscribeStreamContext> SubscribeStreamContexts)> StartDirectPublishingAsync(IRtmpPublishStreamContext publishStreamContext);
        ValueTask<(bool Result, IList<IRtmpSubscribeStreamContext> SubscribeStreamContexts)> StopPublishingAsync(IRtmpPublishStreamContext publishStreamContext, bool allowContinuation = true);

        ValueTask<(SubscribingStreamResult Result, IRtmpPublishStreamContext? PublishStreamContext)> StartSubscribingAsync(IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask<bool> StopSubscribingAsync(IRtmpSubscribeStreamContext subscribeStreamContext);

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
