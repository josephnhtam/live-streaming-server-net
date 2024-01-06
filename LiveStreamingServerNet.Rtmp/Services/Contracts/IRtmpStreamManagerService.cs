using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    internal interface IRtmpStreamManagerService
    {
        bool IsStreamPathPublishing(string publishStreamPath);
        string? GetPublishStreamPath(IRtmpClientPeerContext publisherPeerContext);
        IRtmpClientPeerContext? GetPublishingClientPeerContext(string publishStreamPath);
        IRtmpPublishStreamContext? GetPublishStreamContext(string publishStreamPath);

        PublishingStreamResult StartPublishingStream(IRtmpClientPeerContext publisherPeerContext, string streamPath, IDictionary<string, string> streamArguments, out IList<IRtmpClientPeerContext> existingSubscribers);
        bool StopPublishingStream(IRtmpClientPeerContext publisherPeerContext, out IList<IRtmpClientPeerContext> existingSubscribers);

        SubscribingStreamResult StartSubscribingStream(IRtmpClientPeerContext subscriberPeerContext, uint chunkStreamId, string streamPath, IDictionary<string, string> streamArguments);
        bool StopSubscribingStream(IRtmpClientPeerContext subscriberPeerContext);

        IRentable<IList<IRtmpClientPeerContext>> GetSubscribersLocked(string publishStreamPath);
        IList<IRtmpClientPeerContext> GetSubscribers(string publishStreamPath);
    }
}
