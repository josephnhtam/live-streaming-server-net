using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using Nito.AsyncEx;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvStreamManagerService : IFlvStreamManagerService
    {
        private readonly AsyncReaderWriterLock _publishingRwLock = new();
        private readonly Dictionary<string, IFlvStreamContext> _publishingStreamContexts = new();

        private readonly AsyncReaderWriterLock _subscribingRwLock = new();
        private readonly Dictionary<string, List<IFlvClient>> _subscribingClients = new();
        private readonly Dictionary<IFlvClient, string> _subscribedStreamPaths = new();

        public bool IsStreamPathPublishing(string streamPath, bool requireReady)
        {
            using var readLock = _publishingRwLock.ReaderLock();
            return _publishingStreamContexts.TryGetValue(streamPath, out var streamContext) && (!requireReady || streamContext.IsReady);
        }

        public PublishingStreamResult StartPublishingStream(IFlvStreamContext streamContext)
        {
            using var writeLock = _publishingRwLock.WriterLock();
            var streamPath = streamContext.StreamPath;

            if (_publishingStreamContexts.ContainsKey(streamPath))
                return PublishingStreamResult.AlreadyExists;

            _publishingStreamContexts.Add(streamPath, streamContext);
            return PublishingStreamResult.Succeeded;
        }

        public bool StopPublishingStream(string streamPath, out IList<IFlvClient> existingSubscribers)
        {
            using var publishingWriteLock = _publishingRwLock.WriterLock();
            using var subscribingWriteLock = _subscribingRwLock.WriterLock();

            existingSubscribers = null!;

            if (!_publishingStreamContexts.Remove(streamPath, out var streamContext))
                return false;

            existingSubscribers = _subscribingClients.Remove(streamPath, out var outExistingSubscribers) ?
                outExistingSubscribers : new List<IFlvClient>();

            foreach (var subscriber in existingSubscribers)
                _subscribedStreamPaths.Remove(subscriber);

            return true;
        }

        public IFlvStreamContext? GetFlvStreamContext(string streamPath)
        {
            using var readLock = _publishingRwLock.WriterLock();
            return _publishingStreamContexts.GetValueOrDefault(streamPath);
        }

        public SubscribingStreamResult StartSubscribingStream(IFlvClient client, string streamPath)
        {
            using var publishingReadLock = _publishingRwLock.ReaderLock();
            using var subscribingWriteLock = _subscribingRwLock.WriterLock();

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

        public bool StopSubscribingStream(IFlvClient client)
        {
            using var subscribingWriteLock = _subscribingRwLock.WriterLock();

            if (!_subscribedStreamPaths.Remove(client, out var streamPath))
                return false;

            if (_subscribingClients.TryGetValue(streamPath, out var subscribers) &&
                subscribers.Remove(client) && subscribers.Count == 0)
                _subscribingClients.Remove(streamPath);

            return true;
        }

        public IRentable<IReadOnlyList<IFlvClient>> GetSubscribersLocked(string streamPath)
        {
            var readLock = _subscribingRwLock.ReaderLock();

            return new Rentable<IReadOnlyList<IFlvClient>>(
                _subscribingClients.GetValueOrDefault(streamPath)?.ToList() ?? new List<IFlvClient>(),
                readLock.Dispose);
        }

        public IReadOnlyList<IFlvClient> GetSubscribers(string streamPath)
        {
            using var readLock = _subscribingRwLock.ReaderLock();
            return _subscribingClients.GetValueOrDefault(streamPath)?.ToList() ?? new List<IFlvClient>();
        }
    }
}
