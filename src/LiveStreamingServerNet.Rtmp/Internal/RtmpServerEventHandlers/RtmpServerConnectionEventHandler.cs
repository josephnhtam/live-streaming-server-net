using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
{
    internal class RtmpServerConnectionEventHandler : IRtmpServerConnectionEventHandler
    {
        private readonly IRtmpMediaMessageBroadcasterService _mediaMessageBroadcaster;
        private readonly IRtmpStreamDeletionService _streamDeletionService;

        public RtmpServerConnectionEventHandler(
            IRtmpMediaMessageBroadcasterService mediaMessageBroadcaster,
            IRtmpStreamDeletionService streamDeletionService)
        {
            _mediaMessageBroadcaster = mediaMessageBroadcaster;
            _streamDeletionService = streamDeletionService;
        }

        public ValueTask OnRtmpClientConnectedAsync(IEventContext context, IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            _mediaMessageBroadcaster.RegisterClient(clientContext);
            return ValueTask.CompletedTask;
        }

        public async ValueTask OnRtmpClientDisposedAsync(IEventContext context, IRtmpClientContext clientContext)
        {
            _mediaMessageBroadcaster.UnregisterClient(clientContext);
            await _streamDeletionService.DeleteStreamAsync(clientContext);
        }

        public ValueTask OnRtmpClientCreatedAsync(IEventContext context, IRtmpClientContext clientContext)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpClientHandshakeCompleteAsync(IEventContext context, IRtmpClientContext clientId)
        {
            return ValueTask.CompletedTask;
        }
    }
}
