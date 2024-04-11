using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using Nito.AsyncEx;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpStreamManagerService : IRtmpStreamManagerService
    {
        private readonly AsyncReaderWriterLock _publishingRwLock = new();
        private readonly Dictionary<IRtmpClientContext, string> _publishStreamPaths = new();
        private readonly Dictionary<string, IRtmpClientContext> _publishingClientContexts = new();

        private readonly AsyncReaderWriterLock _subscribingRwLock = new();
        private readonly Dictionary<string, List<IRtmpClientContext>> _subscribingClientContexts = new();
        private readonly Dictionary<IRtmpClientContext, string> _subscribedStreamPaths = new();

        public string? GetPublishStreamPath(IRtmpClientContext publisherClientContext)
        {
            return _publishStreamPaths.GetValueOrDefault(publisherClientContext);
        }

        public IRtmpClientContext? GetPublishingClientContext(string streamPath)
        {
            using var readLock = _publishingRwLock.ReaderLock();
            return _publishingClientContexts.GetValueOrDefault(streamPath);
        }

        public IRtmpPublishStreamContext? GetPublishStreamContext(string streamPath)
        {
            var publishingClientContext = GetPublishingClientContext(streamPath);
            return publishingClientContext?.PublishStreamContext;
        }

        public PublishingStreamResult StartPublishingStream(IRtmpClientContext publisherClientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IList<IRtmpClientContext> existingSubscribers)
        {
            using var publishingWriteLock = _publishingRwLock.WriterLock();
            using var subscribingReadLock = _subscribingRwLock.ReaderLock();

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
            using var publishingWriteLock = _publishingRwLock.WriterLock();
            using var subscribingWriteLock = _subscribingRwLock.WriterLock();

            existingSubscribers = null!;

            if (!_publishStreamPaths.TryGetValue(publisherClientContext, out var streamPath))
                return false;

            _publishingClientContexts.Remove(streamPath);
            _publishStreamPaths.Remove(publisherClientContext);

            existingSubscribers = _subscribingClientContexts.GetValueOrDefault(streamPath)?.ToList() ?? new List<IRtmpClientContext>();

            return true;
        }

        public bool IsStreamPathPublishing(string streamPath)
        {
            using var readLock = _publishingRwLock.ReaderLock();
            return _publishingClientContexts.ContainsKey(streamPath);
        }

        public SubscribingStreamResult StartSubscribingStream(IRtmpClientContext subscriberClientContext, uint chunkStreamId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            using var publishingReadLock = _publishingRwLock.ReaderLock();
            using var subscribingWriteLock = _subscribingRwLock.WriterLock();

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
            using var subscribingWriteLock = _subscribingRwLock.WriterLock();

            if (!_subscribedStreamPaths.Remove(subscriberClientContext, out var streamPath))
                return false;

            if (_subscribingClientContexts.TryGetValue(streamPath, out var subscribers) &&
                subscribers.Remove(subscriberClientContext) && subscribers.Count == 0)
                _subscribingClientContexts.Remove(streamPath);

            return true;
        }

        public IRentable<IReadOnlyList<IRtmpClientContext>> GetSubscribersLocked(string streamPath)
        {
            var readLock = _subscribingRwLock.ReaderLock();

            return new Rentable<IReadOnlyList<IRtmpClientContext>>(
                _subscribingClientContexts.GetValueOrDefault(streamPath)?.ToList() ?? new List<IRtmpClientContext>(),
                readLock.Dispose);
        }

        public IReadOnlyList<IRtmpClientContext> GetSubscribers(string streamPath)
        {
            using var readLock = _subscribingRwLock.ReaderLock();
            return _subscribingClientContexts.GetValueOrDefault(streamPath)?.ToList() ?? new List<IRtmpClientContext>();
        }
    }
}
