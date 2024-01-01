using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpServerContext
    {
        bool IsStreamPathPublishing(string publishStreamPath);
        string? GetPublishStreamPath(IRtmpClientPeerContext publisherPeerContext);
        IRtmpClientPeerContext? GetPublishingClientPeerContext(string publishStreamPath);

        PublishingStreamResult StartPublishingStream(IRtmpClientPeerContext publisherPeerContext, string streamPath, IDictionary<string, string> streamArguments);
        void StopPublishingStream(string publishStreamPath, out IList<IRtmpClientPeerContext> existingSubscribers);

        SubscribingStreamResult StartSubscribingStream(IRtmpClientPeerContext subscriberPeerContext, uint chunkStreamId, string streamPath, IDictionary<string, string> streamArguments);
        void StopSubscribingStream(IRtmpClientPeerContext subscriberPeerContext);

        IRentable<IList<IRtmpClientPeerContext>> GetSubscribersLocked(string publishStreamPath);
        IList<IRtmpClientPeerContext> GetSubscribers(string publishStreamPath);

        void RemoveClientPeerContext(IRtmpClientPeerContext peerContext);
    }
}
