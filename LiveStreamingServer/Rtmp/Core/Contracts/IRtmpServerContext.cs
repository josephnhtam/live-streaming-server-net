using LiveStreamingServer.Utilities.Contracts;

namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IRtmpServerContext
    {
        bool IsStreamPathPublishing(string publishStreamPath);
        string? GetPublishStreamPath(IRtmpClientPeerContext publisherPeerContext);
        IRtmpClientPeerContext? GetPublishingClientPeerContext(string publishStreamPath);

        PublishingStreamResult StartPublishingStream(string publishStreamPath, IRtmpClientPeerContext publisherPeerContext);
        void StopPublishingStream(string publishStreamPath, out IList<IRtmpClientPeerContext> existingSubscribers);

        SubscribingStreamResult StartSubscribingStream(string publishStreamPath, IRtmpClientPeerContext subscriberPeerContext);
        void StopSubscribingStream(IRtmpClientPeerContext subscriberPeerContext);

        IRentable<IList<IRtmpClientPeerContext>> GetSubscribersLocked(string publishStreamPath);
        IList<IRtmpClientPeerContext> GetSubscribers(string publishStreamPath);

        void RemoveClientPeerContext(IRtmpClientPeerContext peerContext);
    }
}
