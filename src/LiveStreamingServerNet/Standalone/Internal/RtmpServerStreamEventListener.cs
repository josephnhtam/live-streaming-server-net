using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Standalone.Internal
{
    internal class RtmpServerStreamEventListener : IRtmpServerStreamEventHandler
    {
        private readonly IRtmpStreamManagerService _streamManager;

        public RtmpServerStreamEventListener(IRtmpStreamManagerService streamManager)
        {
            _streamManager = streamManager;
        }

        public async ValueTask OnRtmpStreamMetaDataReceived(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            await _streamManager.RtmpStreamMetaDataReceived(clientId, streamPath, metaData);
        }

        public async ValueTask OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            await _streamManager.RtmpStreamPublishedAsync(clientId, streamPath, streamArguments);
        }

        public async ValueTask OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            await _streamManager.RtmpStreamSubscribedAsync(clientId, streamPath, streamArguments);
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            await _streamManager.RtmpStreamUnpublishedAsync(clientId, streamPath);
        }

        public async ValueTask OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
        {
            await _streamManager.RtmpStreamUnsubscribedAsync(clientId, streamPath);
        }
    }
}
