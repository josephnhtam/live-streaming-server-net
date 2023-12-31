using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Utilities;
using LiveStreamingServer.Utilities.Contracts;
using Open.Threading;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpServerContext : IRtmpServerContext
    {
        private readonly ReaderWriterLockSlim _publishingRwLock = new();
        private readonly Dictionary<IRtmpClientPeerContext, string> _publishStreamPaths = new();
        private readonly Dictionary<string, IRtmpClientPeerContext> _publishingClientPeerContexts = new();

        private readonly ReaderWriterLockSlim _subscribingRwLock = new();
        private readonly Dictionary<string, List<IRtmpClientPeerContext>> _subscribingClientPeerContexts = new();
        private readonly Dictionary<IRtmpClientPeerContext, string> _subscribedStreamPaths = new();

        public string? GetPublishStreamPath(IRtmpClientPeerContext publisherPeerContext)
        {
            return _publishStreamPaths.GetValueOrDefault(publisherPeerContext);
        }

        public IRtmpClientPeerContext? GetPublishingClientPeerContext(string publishStreamPath)
        {
            using var readLock = _publishingRwLock.ReadLock();
            return _publishingClientPeerContexts.GetValueOrDefault(publishStreamPath);
        }

        public PublishingStreamResult StartPublishingStream(string publishStreamPath, IRtmpClientPeerContext publisherPeerContext)
        {
            using var readLock = _publishingRwLock.WriteLock();

            if (_publishStreamPaths.ContainsKey(publisherPeerContext))
                return PublishingStreamResult.AlreadyPublishing;

            if (_publishingClientPeerContexts.ContainsKey(publishStreamPath))
                return PublishingStreamResult.AlreadyExists;

            _publishStreamPaths.Add(publisherPeerContext, publishStreamPath);
            _publishingClientPeerContexts.Add(publishStreamPath, publisherPeerContext);

            return PublishingStreamResult.Succeeded;
        }

        public void StopPublishingStream(string publishStreamPath, out IList<IRtmpClientPeerContext> existingSubscribers)
        {
            using var publishingWriteLock = _publishingRwLock.WriteLock();
            using var subscribingWriteLock = _subscribingRwLock.WriteLock();

            existingSubscribers = _subscribingClientPeerContexts.GetValueOrDefault(publishStreamPath)?.ToList()
                ?? new List<IRtmpClientPeerContext>();

            if (!_publishingClientPeerContexts.TryGetValue(publishStreamPath, out var peerContext))
                return;

            _subscribingClientPeerContexts.Remove(publishStreamPath);
            _publishingClientPeerContexts.Remove(publishStreamPath);
            _publishStreamPaths.Remove(peerContext);
        }

        public bool IsStreamPathPublishing(string publishStreamPath)
        {
            using var readLock = _publishingRwLock.ReadLock();
            return _publishingClientPeerContexts.ContainsKey(publishStreamPath);
        }

        public void RemoveClientPeerContext(IRtmpClientPeerContext peerContext)
        {
            using var writeLock = _publishingRwLock.WriteLock();

            if (!_publishStreamPaths.TryGetValue(peerContext, out var publishStreamPath))
                return;

            _publishStreamPaths.Remove(peerContext);
            _publishingClientPeerContexts.Remove(publishStreamPath);
        }

        public SubscribingStreamResult StartSubscribingStream(string publishStreamPath, IRtmpClientPeerContext subscriberPeerContext)
        {
            using var subscribingWriteLock = _subscribingRwLock.WriteLock();

            if (_subscribingClientPeerContexts.TryGetValue(publishStreamPath, out var subscribers))
            {
                subscribers = new List<IRtmpClientPeerContext>();
                _subscribingClientPeerContexts[publishStreamPath] = subscribers;
            }

            if (_subscribedStreamPaths.ContainsKey(subscriberPeerContext))
                return SubscribingStreamResult.AlreadySubscribing;

            subscribers!.Add(subscriberPeerContext);
            _subscribedStreamPaths.Add(subscriberPeerContext, publishStreamPath);

            return SubscribingStreamResult.Succeeded;
        }

        public void StopSubscribingStream(IRtmpClientPeerContext subscriberPeerContext)
        {
            using var subscribingWriteLock = _subscribingRwLock.WriteLock();

            if (!_subscribedStreamPaths.Remove(subscriberPeerContext, out var publishStreamPath))
                return;

            if (_subscribingClientPeerContexts.TryGetValue(publishStreamPath, out var subscribers))
            {
                subscribers.Remove(subscriberPeerContext);
            }
        }

        public IRentable<IList<IRtmpClientPeerContext>> GetSubscribersLocked(string publishStreamPath)
        {
            var readLock = _subscribingRwLock.ReadLock();

            return new Rentable<IList<IRtmpClientPeerContext>>(
                _subscribingClientPeerContexts.GetValueOrDefault(publishStreamPath)?.ToList() ?? new List<IRtmpClientPeerContext>(),
                readLock.Dispose);
        }

        public IList<IRtmpClientPeerContext> GetSubscribers(string publishStreamPath)
        {
            using var readLock = _subscribingRwLock.ReadLock();

            return _subscribingClientPeerContexts.GetValueOrDefault(publishStreamPath)?.ToList() ?? new List<IRtmpClientPeerContext>();
        }
    }

    public enum PublishingStreamResult
    {
        Succeeded,
        AlreadyExists,
        AlreadyPublishing,
    }

    public enum SubscribingStreamResult
    {
        Succeeded,
        AlreadySubscribing,
    }
}
