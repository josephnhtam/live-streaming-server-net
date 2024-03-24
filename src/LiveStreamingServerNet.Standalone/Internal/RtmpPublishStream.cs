using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Standalone.Internal
{
    internal class RtmpPublishStream : IRtmpPublishStream
    {
        public string Id { get; }
        public IClientControl Client { get; }
        public string StreamPath { get; }
        public DateTime StartTime { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }
        public IReadOnlyDictionary<string, object>? MetaData { get; private set; }
        public IReadOnlyList<IClientControl> Subscribers => _subscribers.Values.ToList();
        public int SubscribersCount => _subscribers.Count;

        private ConcurrentDictionary<uint, IClientControl> _subscribers = new();

        public RtmpPublishStream(IClientControl client, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            Id = Guid.NewGuid().ToString();
            Client = client;
            StreamPath = streamPath;
            StartTime = DateTime.UtcNow;
            StreamArguments = new Dictionary<string, string>(streamArguments);
        }

        public void UpdateMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            MetaData = new Dictionary<string, object>(metaData);
        }

        public bool AddSubscriber(IClientControl client)
        {
            return _subscribers.TryAdd(client.ClientId, client);
        }

        public bool RemoveSubscriber(uint clientId)
        {
            return _subscribers.TryRemove(clientId, out _);
        }
    }
}

