using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using Open.Threading;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvStreamManagerService : IFlvStreamManagerService
    {
        private readonly ReaderWriterLockSlim _publishingRwLock = new();
        private readonly Dictionary<string, IFlvStreamContext> _publishingStreamContexts = new();

        private readonly ReaderWriterLockSlim _subscribingRwLock = new();
        private readonly Dictionary<string, List<IFlvClient>> _subscribingClients = new();
        private readonly Dictionary<IFlvClient, string> _subscribedStreamPaths = new();

        public bool IsStreamPathPublishing(string publishStreamPath)
        {
            using var readLock = _publishingRwLock.ReadLock();
            return _publishingStreamContexts.ContainsKey(publishStreamPath);
        }

        public PublishingStreamResult StartPublishingStream(IFlvStreamContext streamContext, string streamPath, IDictionary<string, string> streamArguments)
        {
            using var writeLock = _publishingRwLock.WriteLock();

            if (_publishingStreamContexts.ContainsKey(streamPath))
                return PublishingStreamResult.AlreadyExists;

            _publishingStreamContexts.Add(streamPath, streamContext);
            return PublishingStreamResult.Succeeded;
        }

        public bool StopPublishingStream(string streamPath, out IList<IFlvClient> existingSubscribers)
        {
            using var publishingWriteLock = _publishingRwLock.WriteLock();
            using var subscribingWriteLock = _subscribingRwLock.WriteLock();

            existingSubscribers = null!;

            if (!_publishingStreamContexts.Remove(streamPath, out var streamContext))
                return false;

            existingSubscribers = _subscribingClients.Remove(streamPath, out var outExistingSubscribers) ?
                outExistingSubscribers : new List<IFlvClient>();

            foreach (var subscriber in existingSubscribers)
                _subscribedStreamPaths.Remove(subscriber);

            return true;
        }

        public SubscribingStreamResult StartSubscribingStream(IFlvClient client, string streamPath)
        {
            using var publishingReadLock = _publishingRwLock.ReadLock();
            using var subscribingWriteLock = _subscribingRwLock.WriteLock();

            if (_publishingStreamContexts.ContainsKey(streamPath))
                return SubscribingStreamResult.StreamDoesntExist;

            if (_subscribedStreamPaths.ContainsKey(client))
                return SubscribingStreamResult.AlreadySubscribing;

            if (!_subscribingClients.TryGetValue(streamPath, out var subscribers))
            {
                subscribers = new List<IFlvClient>();
                _subscribingClients[streamPath] = subscribers;
            }

            subscribers.Add(client);

            return SubscribingStreamResult.Succeeded;
        }

        public bool StopSubscribingStream(IFlvClient client)
        {
            using var subscribingWriteLock = _subscribingRwLock.WriteLock();

            if (!_subscribedStreamPaths.Remove(client, out var publishStreamPath))
                return false;

            if (_subscribingClients.TryGetValue(publishStreamPath, out var subscribers))
            {
                if (subscribers.Remove(client) && subscribers.Count == 0)
                    _subscribingClients.Remove(publishStreamPath);
            }

            return true;
        }
    }
}
