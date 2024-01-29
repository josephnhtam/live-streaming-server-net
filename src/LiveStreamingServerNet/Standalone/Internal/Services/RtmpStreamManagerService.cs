using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Events;
using LiveStreamingServerNet.Standalone.Internal.Services.Contracts;
using MediatR;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace LiveStreamingServerNet.Standalone.Internal.Services
{
    internal class RtmpStreamManagerService : IRtmpStreamManagerService
    {
        private readonly IServer _server;
        private readonly IMediator _mediator;
        private readonly ConcurrentDictionary<string, RtmpPublishStream> _publishStreams = new();
        private readonly ConcurrentDictionary<string, string> _streamPaths = new();

        public RtmpStreamManagerService(IServer server, IMediator mediator)
        {
            _server = server;
            _mediator = mediator;
        }

        private IClientControl GetClient(uint clientId)
        {
            var client = _server.GetClient(clientId);
            Debug.Assert(client != null, $"Client ({clientId}) not found");
            return client;
        }

        public async Task RtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var client = GetClient(clientId);
            var stream = new RtmpPublishStream(client, streamPath, new Dictionary<string, string>(streamArguments));

            if (_publishStreams.TryAdd(streamPath, stream))
            {
                _streamPaths[stream.Id] = streamPath;
                await _mediator.Publish(new RtmpStreamPublishedEvent(stream));
            }
        }

        public async Task RtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            if (_publishStreams.TryRemove(streamPath, out var stream))
            {
                _streamPaths.TryRemove(stream.Id, out _);
                await _mediator.Publish(new RtmpStreamUnpublishedEvent(stream));
            }
        }

        public async Task RtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var subscriberClient = GetClient(clientId);
            if (_publishStreams.TryGetValue(streamPath, out var publishStream) && publishStream.AddSubscriber(subscriberClient))
                await _mediator.Publish(new RtmpStreamSubscribedEvent(publishStream, subscriberClient));
        }

        public async Task RtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
        {
            var subscriberClient = GetClient(clientId);
            if (_publishStreams.TryGetValue(streamPath, out var publishStream) && publishStream.RemoveSubscriber(clientId))
                await _mediator.Publish(new RtmpStreamUnsubscribedEvent(publishStream, subscriberClient));
        }

        public async Task RtmpStreamMetaDataReceived(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            if (_publishStreams.TryGetValue(streamPath, out var publishStream))
            {
                publishStream.UpdateMetaData(metaData);
                await _mediator.Publish(new RtmpStreamMetaDataReceivedEvent(publishStream));
            }
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
