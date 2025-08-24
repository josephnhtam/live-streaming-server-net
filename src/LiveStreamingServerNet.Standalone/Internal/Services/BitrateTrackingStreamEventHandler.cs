using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Standalone.Internal.Services
{
    internal class BitrateTrackingStreamEventHandler : IRtmpServerStreamEventHandler
    {
        private readonly IBitrateTrackingService _bitrateTrackingService;

        public BitrateTrackingStreamEventHandler(IBitrateTrackingService bitrateTrackingService)
        {
            _bitrateTrackingService = bitrateTrackingService;
        }

        public ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath)
        {
            _bitrateTrackingService.CleanupStream(streamPath);
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamPublishedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, uint clientId, string streamPath)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
            => ValueTask.CompletedTask;
    }
}
