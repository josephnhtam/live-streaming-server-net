using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Standalone.Internal
{
    internal class RtmpServerStreamEventListener : IRtmpServerStreamEventHandler
    {
        private readonly IRtmpStreamManagerService _streamManager;

        public RtmpServerStreamEventListener(IRtmpStreamManagerService streamManager)
        {
            _streamManager = streamManager;
        }

        public async ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            await _streamManager.RtmpStreamMetaDataReceivedAsync(clientId, streamPath, metaData);
        }

        public async ValueTask OnRtmpStreamPublishedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            await _streamManager.RtmpStreamPublishedAsync(clientId, streamPath, streamArguments);
        }

        public async ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            await _streamManager.RtmpStreamSubscribedAsync(clientId, streamPath, streamArguments);
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath)
        {
            await _streamManager.RtmpStreamUnpublishedAsync(clientId, streamPath);
        }

        public async ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, uint clientId, string streamPath)
        {
            await _streamManager.RtmpStreamUnsubscribedAsync(clientId, streamPath);
        }
    }
}
