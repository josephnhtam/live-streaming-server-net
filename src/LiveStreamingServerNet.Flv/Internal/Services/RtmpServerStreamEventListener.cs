using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;

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
            var allowContinuation = context.Items!.GetValueOrDefault("AllowContinuation", false);
            _streamManager.StopPublishingStream(streamPath, allowContinuation, out _);
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
