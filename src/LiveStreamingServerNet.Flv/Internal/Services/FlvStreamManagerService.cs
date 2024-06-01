using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvStreamManagerService : IFlvStreamManagerService
    {
        private readonly object _publishingSyncLock = new();
        private readonly Dictionary<string, IFlvStreamContext> _publishingStreamContexts = new();

        private readonly object _subscribingSyncLock = new();
        private readonly Dictionary<string, List<IFlvClient>> _subscribingClients = new();
        private readonly Dictionary<IFlvClient, string> _subscribedStreamPaths = new();

        public bool IsStreamPathPublishing(string streamPath, bool requireReady)
        {
            lock (_publishingSyncLock)
            {
                return _publishingStreamContexts.TryGetValue(streamPath, out var streamContext) && (!requireReady || streamContext.IsReady);
            }
        }

        public PublishingStreamResult StartPublishingStream(IFlvStreamContext streamContext)
        {
            lock (_publishingSyncLock)
            {
                var streamPath = streamContext.StreamPath;

                if (_publishingStreamContexts.ContainsKey(streamPath))
                {
                    streamContext.Dispose();
                    return PublishingStreamResult.AlreadyExists;
                }

                _publishingStreamContexts.Add(streamPath, streamContext);
                return PublishingStreamResult.Succeeded;
            }
        }

        public bool StopPublishingStream(string streamPath, out IList<IFlvClient> existingSubscribers)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    existingSubscribers = null!;

                    if (!_publishingStreamContexts.Remove(streamPath, out var streamContext))
                        return false;

                    existingSubscribers = _subscribingClients.Remove(streamPath, out var outExistingSubscribers) ?
                        outExistingSubscribers : new List<IFlvClient>();

                    foreach (var subscriber in existingSubscribers)
                        _subscribedStreamPaths.Remove(subscriber);

                    streamContext.Dispose();

                    return true;
                }
            }
        }

        public IFlvStreamContext? GetFlvStreamContext(string streamPath)
        {
            lock (_publishingSyncLock)
            {
                return _publishingStreamContexts.GetValueOrDefault(streamPath);
            }
        }

        public SubscribingStreamResult StartSubscribingStream(IFlvClient client, string streamPath)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    if (_subscribedStreamPaths.ContainsKey(client))
                        return SubscribingStreamResult.AlreadySubscribing;

                    if (!_publishingStreamContexts.TryGetValue(streamPath, out var stream) || !stream.IsReady)
                        return SubscribingStreamResult.StreamDoesntExist;

                    if (!_subscribingClients.TryGetValue(streamPath, out var subscribers))
                    {
                        subscribers = new List<IFlvClient>();
                        _subscribingClients[streamPath] = subscribers;
                    }

                    subscribers.Add(client);
                    _subscribedStreamPaths[client] = streamPath;

                    return SubscribingStreamResult.Succeeded;
                }
            }
        }

        public bool StopSubscribingStream(IFlvClient client)
        {
            lock (_subscribingSyncLock)
            {
                if (!_subscribedStreamPaths.Remove(client, out var streamPath))
                    return false;

                if (_subscribingClients.TryGetValue(streamPath, out var subscribers) &&
                    subscribers.Remove(client) && subscribers.Count == 0)
                    _subscribingClients.Remove(streamPath);

                return true;
            }
        }

        public IReadOnlyList<IFlvClient> GetSubscribers(string streamPath)
        {
            lock (_subscribingSyncLock)
            {
                return _subscribingClients.GetValueOrDefault(streamPath)?.ToList() ?? new List<IFlvClient>();
            }
        }
    }
}
