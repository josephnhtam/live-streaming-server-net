using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Services.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Standalone.Internal.Services
{
    internal class RtmpStreamManagerService : IRtmpStreamManagerService
    {
        private readonly IServer _server;
        private readonly object _syncLock = new();
        private readonly Dictionary<string, RtmpPublishStream> _publishStreamMap = new();
        private readonly Dictionary<string, List<IClientControl>> _pendingSubscribersMap = new();
        private readonly ConcurrentDictionary<string, string> _streamPaths = new();

        public RtmpStreamManagerService(IServer server)
        {
            _server = server;
        }

        private IClientControl? GetClient(uint clientId)
        {
            return _server.GetClient(clientId);
        }

        public ValueTask RtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var client = GetClient(clientId);

            if (client == null)
                return ValueTask.CompletedTask;

            lock (_syncLock)
            {
                if (!_publishStreamMap.ContainsKey(streamPath))
                {
                    var stream = new RtmpPublishStream(client, streamPath, streamArguments);

                    if (_pendingSubscribersMap.TryGetValue(streamPath, out var pendingSubscribers))
                    {
                        foreach (var subscriber in pendingSubscribers)
                            stream.AddSubscriber(subscriber);

                        _pendingSubscribersMap.Remove(streamPath);
                    }

                    _publishStreamMap[streamPath] = stream;
                    _streamPaths[stream.Id] = streamPath;
                }
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask RtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            lock (_syncLock)
            {
                if (_publishStreamMap.Remove(streamPath, out var stream))
                {
                    _streamPaths.TryRemove(stream.Id, out _);
                }
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask RtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var subscriberClient = GetClient(clientId);
            if (subscriberClient == null)
                return ValueTask.CompletedTask;

            lock (_syncLock)
            {
                if (_publishStreamMap.TryGetValue(streamPath, out var publishStream))
                {
                    publishStream.AddSubscriber(subscriberClient);
                }
                else
                {
                    if (_pendingSubscribersMap.TryGetValue(streamPath, out var pendingSubscribers))
                    {
                        pendingSubscribers.Add(subscriberClient);
                    }
                    else
                    {
                        _pendingSubscribersMap[streamPath] = new List<IClientControl> { subscriberClient };
                    }
                }
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask RtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
        {
            lock (_syncLock)
            {
                if (_publishStreamMap.TryGetValue(streamPath, out var publishStream))
                {
                    publishStream.RemoveSubscriber(clientId);
                }
                else if (_pendingSubscribersMap.TryGetValue(streamPath, out var pendingSubscribers))
                {
                    pendingSubscribers.RemoveAll(x => x.ClientId == clientId);

                    if (!pendingSubscribers.Any())
                        _pendingSubscribersMap.Remove(streamPath);
                }
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask RtmpStreamMetaDataReceivedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            lock (_syncLock)
            {
                if (_publishStreamMap.TryGetValue(streamPath, out var publishStream))
                {
                    publishStream.UpdateMetaData(metaData);
                }
            }

            return ValueTask.CompletedTask;
        }

        public IRtmpPublishStream? GetStream(string id)
        {
            var streamPath = _streamPaths.GetValueOrDefault(id);

            if (streamPath == null)
                return null;

            lock (_syncLock)
            {
                return _publishStreamMap.GetValueOrDefault(streamPath);
            }
        }

        public IList<IRtmpPublishStream> GetStreams()
        {
            lock (_syncLock)
            {
                return _publishStreamMap.Values.ToList<IRtmpPublishStream>();
            }
        }
    }
}
