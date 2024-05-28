using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Services
{
    internal class RtmpServerStreamEventListener : IRtmpServerStreamEventHandler
    {
        private readonly IStreamProcessorManager _streamProcessorManager;

        public RtmpServerStreamEventListener(IStreamProcessorManager streamProcessorManager)
        {
            _streamProcessorManager = streamProcessorManager;
        }

        public async ValueTask OnRtmpStreamPublishedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            await _streamProcessorManager.StartProcessingStreamAsync(clientId, streamPath, streamArguments);
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath)
        {
            await _streamProcessorManager.StopProcessingStreamAsync(clientId, streamPath);
        }

        public ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
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
