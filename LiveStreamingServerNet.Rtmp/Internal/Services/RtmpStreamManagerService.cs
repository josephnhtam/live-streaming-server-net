using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using Open.Threading;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpStreamManagerService : IRtmpStreamManagerService
    {
        private readonly ReaderWriterLockSlim _publishingRwLock = new();
        private readonly Dictionary<IRtmpClientContext, string> _publishStreamPaths = new();
        private readonly Dictionary<string, IRtmpClientContext> _publishingClientContexts = new();

        private readonly ReaderWriterLockSlim _subscribingRwLock = new();
        private readonly Dictionary<string, List<IRtmpClientContext>> _subscribingClientContexts = new();
        private readonly Dictionary<IRtmpClientContext, string> _subscribedStreamPaths = new();

        public string? GetPublishStreamPath(IRtmpClientContext publisherClientContext)
        {
            return _publishStreamPaths.GetValueOrDefault(publisherClientContext);
        }

        public IRtmpClientContext? GetPublishingClientContext(string publishStreamPath)
        {
            using var readLock = _publishingRwLock.ReadLock();
            return _publishingClientContexts.GetValueOrDefault(publishStreamPath);
        }

        public IRtmpPublishStreamContext? GetPublishStreamContext(string publishStreamPath)
        {
            var publishingClientContext = GetPublishingClientContext(publishStreamPath);
            return publishingClientContext?.PublishStreamContext;
        }

        public PublishingStreamResult StartPublishingStream(IRtmpClientContext publisherClientContext, string streamPath, IDictionary<string, string> streamArguments, out IList<IRtmpClientContext> existingSubscribers)
        {
            using var publishingWriteLock = _publishingRwLock.WriteLock();
            using var subscribingReadLock = _subscribingRwLock.ReadLock();

            existingSubscribers = null!;

            if (_subscribedStreamPaths.ContainsKey(publisherClientContext))
                return PublishingStreamResult.AlreadySubscribing;

            if (_publishStreamPaths.ContainsKey(publisherClientContext))
                return PublishingStreamResult.AlreadyPublishing;

            if (_publishingClientContexts.ContainsKey(streamPath))
                return PublishingStreamResult.AlreadyExists;

            var publishStreamContext = publisherClientContext.CreatePublishStreamContext(streamPath, streamArguments);

            _publishStreamPaths.Add(publisherClientContext, streamPath);
            _publishingClientContexts.Add(streamPath, publisherClientContext);

            existingSubscribers = _subscribingClientContexts.GetValueOrDefault(streamPath)?.ToList() ?? new List<IRtmpClientContext>();

            return PublishingStreamResult.Succeeded;
        }

        public bool StopPublishingStream(IRtmpClientContext publisherClientContext, out IList<IRtmpClientContext> existingSubscribers)
        {
            using var publishingWriteLock = _publishingRwLock.WriteLock();
            using var subscribingWriteLock = _subscribingRwLock.WriteLock();

            existingSubscribers = null!;

            if (!_publishStreamPaths.TryGetValue(publisherClientContext, out var publishStreamPath))
                return false;

            _publishingClientContexts.Remove(publishStreamPath);
            _publishStreamPaths.Remove(publisherClientContext);

            existingSubscribers = _subscribingClientContexts.GetValueOrDefault(publishStreamPath)?.ToList() ?? new List<IRtmpClientContext>();

            return true;
        }

        public bool IsStreamPathPublishing(string publishStreamPath)
        {
            using var readLock = _publishingRwLock.ReadLock();
            return _publishingClientContexts.ContainsKey(publishStreamPath);
        }

        public SubscribingStreamResult StartSubscribingStream(IRtmpClientContext subscriberClientContext, uint chunkStreamId, string streamPath, IDictionary<string, string> streamArguments)
        {
            using var publishingReadLock = _publishingRwLock.ReadLock();
            using var subscribingWriteLock = _subscribingRwLock.WriteLock();

            if (_publishStreamPaths.ContainsKey(subscriberClientContext))
                return SubscribingStreamResult.AlreadyPublishing;

            if (_subscribedStreamPaths.ContainsKey(subscriberClientContext))
                return SubscribingStreamResult.AlreadySubscribing;

            if (!_subscribingClientContexts.TryGetValue(streamPath, out var subscribers))
            {
                subscribers = new List<IRtmpClientContext>();
                _subscribingClientContexts[streamPath] = subscribers;
            }

            subscriberClientContext.CreateStreamSubscriptionContext(chunkStreamId, streamPath, streamArguments);

            subscribers.Add(subscriberClientContext);
            _subscribedStreamPaths.Add(subscriberClientContext, streamPath);

            return SubscribingStreamResult.Succeeded;
        }

        public bool StopSubscribingStream(IRtmpClientContext subscriberClientContext)
        {
            using var subscribingWriteLock = _subscribingRwLock.WriteLock();

            if (!_subscribedStreamPaths.Remove(subscriberClientContext, out var publishStreamPath))
                return false;

            if (_subscribingClientContexts.TryGetValue(publishStreamPath, out var subscribers))
            {
                subscribers.Remove(subscriberClientContext);

                if (subscribers.Count == 0)
                    _subscribingClientContexts.Remove(publishStreamPath);
            }

            return true;
        }

        public IRentable<IList<IRtmpClientContext>> GetSubscribersLocked(string publishStreamPath)
        {
            var readLock = _subscribingRwLock.ReadLock();

            return new Rentable<IList<IRtmpClientContext>>(
                _subscribingClientContexts.GetValueOrDefault(publishStreamPath)?.ToList() ?? new List<IRtmpClientContext>(),
                readLock.Dispose);
        }

        public IList<IRtmpClientContext> GetSubscribers(string publishStreamPath)
        {
            using var readLock = _subscribingRwLock.ReadLock();
            return _subscribingClientContexts.GetValueOrDefault(publishStreamPath)?.ToList() ?? new List<IRtmpClientContext>();
        }
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
