using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class RtmpServerStreamEventListener : IRtmpServerStreamEventHandler
    {
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IBufferPool? _bufferPool;

        public RtmpServerStreamEventListener(IFlvStreamManagerService streamManager, IBufferPool? bufferPool = null)
        {
            _streamManager = streamManager;
            _bufferPool = bufferPool;
        }

        public ValueTask OnRtmpStreamPublishedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var streamContext = new FlvStreamContext(streamPath, streamArguments, _bufferPool);
            _streamManager.StartPublishingStream(streamContext);
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath)
        {
            if (_streamManager.StopPublishingStream(streamPath, out var existingSubscribers))
            {
                foreach (var subscriber in existingSubscribers)
                    subscriber.Stop();
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);

            if (streamContext != null)
                streamContext.StreamMetaData = new Dictionary<string, object>(metaData);

            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, uint clientId, string streamPath)
        {
            return ValueTask.CompletedTask;
        }
    }
}
