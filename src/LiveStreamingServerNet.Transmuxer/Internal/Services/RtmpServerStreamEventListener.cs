using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal.Services
{
    internal class RtmpServerStreamEventListener : IRtmpServerStreamEventHandler
    {
        private readonly ITransmuxerManager _transmuxerManager;

        public RtmpServerStreamEventListener(ITransmuxerManager transmuxerManager)
        {
            _transmuxerManager = transmuxerManager;
        }

        public async ValueTask OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            await _transmuxerManager.StartRemuxingStreamAsync(clientId, streamPath, streamArguments.ToDictionary());
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            await _transmuxerManager.StopRemuxingStreamAsync(clientId, streamPath);
        }

        public ValueTask OnRtmpStreamMetaDataReceived(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
        {
            return ValueTask.CompletedTask;
        }
    }
}
