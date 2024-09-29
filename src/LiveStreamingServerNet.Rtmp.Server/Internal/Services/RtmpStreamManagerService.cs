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
            IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    subscribeStreamContexts = null!;

                    if (streamContext.PublishContext != null)
                        return PublishingStreamResult.AlreadyPublishing;

                    if (streamContext.SubscribeContext != null)
                        return PublishingStreamResult.AlreadySubscribing;

                    if (_publishStreamContexts.ContainsKey(streamPath))
                        return PublishingStreamResult.AlreadyExists;

                    var publishStreamContext = streamContext.CreatePublishContext(streamPath, streamArguments);
                    _publishStreamContexts.Add(streamPath, publishStreamContext);

                    subscribeStreamContexts = _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                        new List<IRtmpSubscribeStreamContext>();

                    foreach (var subscribeStreamContext in subscribeStreamContexts)
                        subscribeStreamContext.ResetTimestamps();

                    return PublishingStreamResult.Succeeded;
                }
            }
        }

        public bool StopPublishing(IRtmpPublishStreamContext publishStreamContext, out IList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    var streamPath = publishStreamContext.StreamPath;

                    if (publishStreamContext.StreamContext.PublishContext != publishStreamContext ||
                        !_publishStreamContexts.Remove(streamPath))
                    {
                        subscribeStreamContexts = new List<IRtmpSubscribeStreamContext>();
                        return false;
                    }

                    publishStreamContext.StreamContext.RemovePublishContext();

                    subscribeStreamContexts = _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
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
            IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IRtmpPublishStreamContext? publishStreamContext)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    publishStreamContext = null;

                    if (streamContext.PublishContext != null)
                        return SubscribingStreamResult.AlreadyPublishing;

                    if (streamContext.SubscribeContext != null)
                        return SubscribingStreamResult.AlreadySubscribing;

                    _publishStreamContexts.TryGetValue(streamPath, out publishStreamContext);

                    if (!_subscribeStreamContexts.TryGetValue(streamPath, out var subscribers))
                    {
                        subscribers = new List<IRtmpSubscribeStreamContext>();
                        _subscribeStreamContexts[streamPath] = subscribers;
                    }

                    var subscribeStreamContext = streamContext.CreateSubscribeContext(streamPath, streamArguments);
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

                if (subscribeStreamContext.StreamContext.SubscribeContext != subscribeStreamContext ||
                    !_subscribeStreamContexts.TryGetValue(streamPath, out var subscribers) ||
                    !subscribers.Remove(subscribeStreamContext))
                {
                    return false;
                }

                subscribeStreamContext.StreamContext.RemoveSubscribeContext();

                if (subscribers.Count == 0)
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
