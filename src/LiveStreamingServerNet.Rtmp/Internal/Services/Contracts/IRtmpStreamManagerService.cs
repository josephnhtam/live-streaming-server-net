using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpStreamManagerService
    {
        bool IsStreamPathPublishing(string streamPath);
        string? GetPublishStreamPath(IRtmpClientContext publisherClientContext);
        IRtmpClientContext? GetPublishingClientContext(string streamPath);
        IRtmpPublishStreamContext? GetPublishStreamContext(string streamPath);

        PublishingStreamResult StartPublishingStream(IRtmpClientContext publisherClientContext, string streamPath, IDictionary<string, string> streamArguments, out IList<IRtmpClientContext> existingSubscribers);
        bool StopPublishingStream(IRtmpClientContext publisherClientContext, out IList<IRtmpClientContext> existingSubscribers);

        SubscribingStreamResult StartSubscribingStream(IRtmpClientContext subscriberClientContext, uint chunkStreamId, string streamPath, IDictionary<string, string> streamArguments);
        bool StopSubscribingStream(IRtmpClientContext subscriberClientContext);

        IRentable<IList<IRtmpClientContext>> GetSubscribersLocked(string streamPath);
        IList<IRtmpClientContext> GetSubscribers(string streamPath);
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
