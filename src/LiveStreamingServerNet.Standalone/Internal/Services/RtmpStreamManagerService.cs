using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Services.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Standalone.Internal.Services
{
    internal class RtmpStreamManagerService : IRtmpStreamManagerService
    {
        private readonly IServer _server;
        private readonly ConcurrentDictionary<string, RtmpPublishStream> _publishStreams = new();
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

            var stream = new RtmpPublishStream(client, streamPath, streamArguments);

            if (_publishStreams.TryAdd(streamPath, stream))
                _streamPaths[stream.Id] = streamPath;

            return ValueTask.CompletedTask;
        }

        public ValueTask RtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            if (_publishStreams.TryRemove(streamPath, out var stream))
                _streamPaths.TryRemove(stream.Id, out _);

            return ValueTask.CompletedTask;
        }

        public ValueTask RtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var subscriberClient = GetClient(clientId);
            if (subscriberClient == null)
                return ValueTask.CompletedTask;

            if (_publishStreams.TryGetValue(streamPath, out var publishStream))
                publishStream.AddSubscriber(subscriberClient);

            return ValueTask.CompletedTask;
        }

        public ValueTask RtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
        {
            if (_publishStreams.TryGetValue(streamPath, out var publishStream))
                publishStream.RemoveSubscriber(clientId);

            return ValueTask.CompletedTask;
        }

        public ValueTask RtmpStreamMetaDataReceivedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            if (_publishStreams.TryGetValue(streamPath, out var publishStream))
                publishStream.UpdateMetaData(metaData);

            return ValueTask.CompletedTask;
        }

        public IRtmpPublishStream? GetStream(string id)
        {
            var streamPath = _streamPaths.GetValueOrDefault(id);

            if (streamPath == null)
                return null;

            return _publishStreams.GetValueOrDefault(streamPath);
        }

        public IList<IRtmpPublishStream> GetStreams()
        {
            return _publishStreams.Values.ToList<IRtmpPublishStream>();
        }
    }
}
