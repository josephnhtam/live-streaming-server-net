using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpStreamManagerService : IRtmpStreamManagerService
    {
        private readonly object _publishingSyncLock = new();
        private readonly Dictionary<IRtmpClientSessionContext, string> _publishStreamPaths = new();
        private readonly Dictionary<string, IRtmpClientSessionContext> _publishingClientContexts = new();

        private readonly object _subscribingSyncLock = new();
        private readonly Dictionary<string, List<IRtmpClientSessionContext>> _subscribingClientContexts = new();
        private readonly Dictionary<IRtmpClientSessionContext, string> _subscribedStreamPaths = new();

        public string? GetPublishStreamPath(IRtmpClientSessionContext publisherClientContext)
        {
            return _publishStreamPaths.GetValueOrDefault(publisherClientContext);
        }

        public IRtmpClientSessionContext? GetPublishingClientContext(string streamPath)
        {
            lock (_publishingSyncLock)
            {
                return _publishingClientContexts.GetValueOrDefault(streamPath);
            }
        }

        public IRtmpPublishStreamContext? GetPublishStreamContext(string streamPath)
        {
            var publishingClientContext = GetPublishingClientContext(streamPath);
            return publishingClientContext?.PublishStreamContext;
        }

        public PublishingStreamResult StartPublishingStream(IRtmpClientSessionContext publisherClientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IList<IRtmpClientSessionContext> existingSubscribers)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
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

                    existingSubscribers = _subscribingClientContexts.GetValueOrDefault(streamPath)?.ToList() ?? new List<IRtmpClientSessionContext>();

                    return PublishingStreamResult.Succeeded;
                }
            }
        }

        public bool StopPublishingStream(IRtmpClientSessionContext publisherClientContext, out IList<IRtmpClientSessionContext> existingSubscribers)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    existingSubscribers = null!;

                    if (!_publishStreamPaths.TryGetValue(publisherClientContext, out var streamPath))
                        return false;

                    _publishingClientContexts.Remove(streamPath);
                    _publishStreamPaths.Remove(publisherClientContext);

                    existingSubscribers = _subscribingClientContexts.GetValueOrDefault(streamPath)?.ToList() ?? new List<IRtmpClientSessionContext>();

                    return true;
                }
            }
        }

        public bool IsStreamPathPublishing(string streamPath)
        {
            lock (_publishingSyncLock)
            {
                return _publishingClientContexts.ContainsKey(streamPath);
            }
        }

        public SubscribingStreamResult StartSubscribingStream(IRtmpClientSessionContext subscriberClientContext, uint chunkStreamId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    if (_publishStreamPaths.ContainsKey(subscriberClientContext))
                        return SubscribingStreamResult.AlreadyPublishing;

                    if (_subscribedStreamPaths.ContainsKey(subscriberClientContext))
                        return SubscribingStreamResult.AlreadySubscribing;

                    if (!_subscribingClientContexts.TryGetValue(streamPath, out var subscribers))
                    {
                        subscribers = new List<IRtmpClientSessionContext>();
                        _subscribingClientContexts[streamPath] = subscribers;
                    }

                    subscriberClientContext.CreateStreamSubscriptionContext(chunkStreamId, streamPath, streamArguments);

                    subscribers.Add(subscriberClientContext);
                    _subscribedStreamPaths.Add(subscriberClientContext, streamPath);

                    return SubscribingStreamResult.Succeeded;
                }
            }
        }

        public bool StopSubscribingStream(IRtmpClientSessionContext subscriberClientContext)
        {
            lock (_subscribingSyncLock)
            {
                if (!_subscribedStreamPaths.Remove(subscriberClientContext, out var streamPath))
                    return false;

                if (_subscribingClientContexts.TryGetValue(streamPath, out var subscribers) &&
                    subscribers.Remove(subscriberClientContext) && subscribers.Count == 0)
                    _subscribingClientContexts.Remove(streamPath);

                return true;
            }
        }

        public IReadOnlyList<string> GetStreamPaths()
        {
            lock (_publishingSyncLock)
            {
                return _publishingClientContexts.Keys.ToList();
            }
        }

        public IRtmpClientSessionContext? GetPublisher(string streamPath)
        {
            lock (_publishingSyncLock)
            {
                return _publishingClientContexts.GetValueOrDefault(streamPath);
            }
        }

        public IReadOnlyList<IRtmpClientSessionContext> GetSubscribers(string streamPath)
        {
            lock (_subscribingSyncLock)
            {
                return _subscribingClientContexts.GetValueOrDefault(streamPath)?.ToList() ?? new List<IRtmpClientSessionContext>();
            }
        }
    }
}
