using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpStreamManagerService
    {
        bool IsStreamPathPublishing(string publishStreamPath);
        string? GetPublishStreamPath(IRtmpClientContext publisherClientContext);
        IRtmpClientContext? GetPublishingClientContext(string publishStreamPath);
        IRtmpPublishStreamContext? GetPublishStreamContext(string publishStreamPath);

        PublishingStreamResult StartPublishingStream(IRtmpClientContext publisherClientContext, string streamPath, IDictionary<string, string> streamArguments, out IList<IRtmpClientContext> existingSubscribers);
        bool StopPublishingStream(IRtmpClientContext publisherClientContext, out IList<IRtmpClientContext> existingSubscribers);

        SubscribingStreamResult StartSubscribingStream(IRtmpClientContext subscriberClientContext, uint chunkStreamId, string streamPath, IDictionary<string, string> streamArguments);
        bool StopSubscribingStream(IRtmpClientContext subscriberClientContext);

        IRentable<IList<IRtmpClientContext>> GetSubscribersLocked(string publishStreamPath);
        IList<IRtmpClientContext> GetSubscribers(string publishStreamPath);
    }
}
