using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal.Services
{
    internal class RtmpServerStreamEventListener : IRtmpServerStreamEventHandler
    {
        private readonly ITransmuxerManager _transmuxerManager;

        public RtmpServerStreamEventListener(ITransmuxerManager transmuxerManager)
        {
            _transmuxerManager = transmuxerManager;
        }

        public async ValueTask OnRtmpStreamPublishedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            await _transmuxerManager.StartRemuxingStreamAsync(clientId, streamPath, streamArguments);
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath)
        {
            await _transmuxerManager.StopRemuxingStreamAsync(clientId, streamPath);
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
