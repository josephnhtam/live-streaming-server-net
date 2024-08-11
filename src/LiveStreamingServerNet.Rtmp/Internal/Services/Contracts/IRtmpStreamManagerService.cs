using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpStreamManagerService
    {
        bool IsStreamPathPublishing(string streamPath);
        string? GetPublishStreamPath(IRtmpClientSessionContext publisherClientContext);
        IRtmpClientSessionContext? GetPublishingClientContext(string streamPath);
        IRtmpPublishStreamContext? GetPublishStreamContext(string streamPath);

        PublishingStreamResult StartPublishingStream(IRtmpClientSessionContext publisherClientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IList<IRtmpClientSessionContext> existingSubscribers);
        bool StopPublishingStream(IRtmpClientSessionContext publisherClientContext, out IList<IRtmpClientSessionContext> existingSubscribers);

        SubscribingStreamResult StartSubscribingStream(IRtmpClientSessionContext subscriberClientContext, uint chunkStreamId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        bool StopSubscribingStream(IRtmpClientSessionContext subscriberClientContext);

        IReadOnlyList<string> GetStreamPaths();
        IRtmpClientSessionContext? GetPublisher(string streamPath);
        IReadOnlyList<IRtmpClientSessionContext> GetSubscribers(string streamPath);
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
