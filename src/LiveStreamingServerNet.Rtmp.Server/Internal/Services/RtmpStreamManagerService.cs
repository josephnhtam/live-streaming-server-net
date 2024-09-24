using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpStreamManagerService : IRtmpStreamManagerService
    {
        private readonly object _publishingSyncLock = new();
        private readonly Dictionary<string, IRtmpPublishStreamContext> _publishStreamContexts = new();

        private readonly object _subscribingSyncLock = new();
        private readonly Dictionary<string, List<IRtmpSubscribeStreamContext>> _subscribeStreamContexts = new();

        public PublishingStreamResult StartPublishing(
            IRtmpStream stream, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IList<IRtmpSubscribeStreamContext> existingSubscribers)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    existingSubscribers = null!;

                    if (stream.PublishContext != null)
                        return PublishingStreamResult.AlreadyPublishing;

                    if (stream.SubscribeContext != null)
                        return PublishingStreamResult.AlreadySubscribing;

                    if (_publishStreamContexts.ContainsKey(streamPath))
                        return PublishingStreamResult.AlreadyExists;

                    var publishStreamContext = stream.CreatePublishContext(streamPath, streamArguments);
                    _publishStreamContexts.Add(streamPath, publishStreamContext);

                    existingSubscribers = _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                        new List<IRtmpSubscribeStreamContext>();

                    return PublishingStreamResult.Succeeded;
                }
            }
        }

        public bool StopPublishing(IRtmpPublishStreamContext publishStreamContext, out IList<IRtmpSubscribeStreamContext> existingSubscribers)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    existingSubscribers = null!;

                    var streamPath = publishStreamContext.StreamPath;

                    _publishStreamContexts.Remove(streamPath);

                    existingSubscribers = _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                        new List<IRtmpSubscribeStreamContext>();

                    return true;
                }
            }
        }

        public bool IsStreamPublishing(string streamPath)
        {
            lock (_publishingSyncLock)
            {
                return _publishStreamContexts.ContainsKey(streamPath);
            }
        }

        public SubscribingStreamResult StartSubscribing(
            IRtmpStream stream, uint chunkStreamId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    if (stream.PublishContext != null)
                        return SubscribingStreamResult.AlreadyPublishing;

                    if (stream.SubscribeContext != null)
                        return SubscribingStreamResult.AlreadySubscribing;

                    if (!_subscribeStreamContexts.TryGetValue(streamPath, out var subscribers))
                    {
                        subscribers = new List<IRtmpSubscribeStreamContext>();
                        _subscribeStreamContexts[streamPath] = subscribers;
                    }

                    var subscribeStreamContext = stream.CreateSubscribeContext(chunkStreamId, streamPath, streamArguments);
                    subscribers.Add(subscribeStreamContext);

                    return SubscribingStreamResult.Succeeded;
                }
            }
        }

        public bool StopSubscribing(IRtmpSubscribeStreamContext subscribeStreamContext)
        {
            lock (_subscribingSyncLock)
            {
                var streamPath = subscribeStreamContext.StreamPath;

                if (_subscribeStreamContexts.TryGetValue(streamPath, out var subscribers) &&
                    subscribers.Remove(subscribeStreamContext) && subscribers.Count == 0)
                    _subscribeStreamContexts.Remove(streamPath);

                return true;
            }
        }

        public IReadOnlyList<string> GetStreamPaths()
        {
            lock (_publishingSyncLock)
            {
                return _publishStreamContexts.Keys.ToList();
            }
        }

        public IRtmpPublishStreamContext? GetPublishStreamContext(string streamPath)
        {
            lock (_publishingSyncLock)
            {
                return _publishStreamContexts.GetValueOrDefault(streamPath);
            }
        }

        public IReadOnlyList<IRtmpSubscribeStreamContext> GetSubscribeStreamContexts(string streamPath)
        {
            lock (_subscribingSyncLock)
            {
                return _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                    new List<IRtmpSubscribeStreamContext>();
            }
        }
    }
}
