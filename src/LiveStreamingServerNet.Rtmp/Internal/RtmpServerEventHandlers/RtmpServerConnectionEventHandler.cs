using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
{
    internal class RtmpServerConnectionEventHandler : IRtmpServerConnectionEventHandler
    {
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;
        private readonly IRtmpStreamDeletionService _streamDeletionService;

        public RtmpServerConnectionEventHandler(
            IRtmpMediaMessageManagerService mediaMessageManager,
            IRtmpStreamDeletionService streamDeletionService)
        {
            _mediaMessageManager = mediaMessageManager;
            _streamDeletionService = streamDeletionService;
        }

        public ValueTask OnRtmpClientConnectedAsync(IEventContext context, IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            _mediaMessageManager.RegisterClient(clientContext);
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpClientDisposedAsync(IEventContext context, IRtmpClientContext clientContext)
        {
            _mediaMessageManager.UnregisterClient(clientContext);
            _streamDeletionService.DeleteStream(clientContext);
            return ValueTask.CompletedTask;
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
