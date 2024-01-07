using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEvents
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

        public Task OnRtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            _mediaMessageManager.RegisterClient(clientContext);
            return Task.CompletedTask;
        }

        public Task OnRtmpClientDisposedAsync(IRtmpClientContext clientContext)
        {
            _mediaMessageManager.UnregisterClient(clientContext);
            _streamDeletionService.DeleteStream(clientContext);
            return Task.CompletedTask;
        }

        public Task OnRtmpClientCreatedAsync(IRtmpClientContext clientContext)
        {
            return Task.CompletedTask;
        }

        public Task OnRtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId)
        {
            return Task.CompletedTask;
        }
    }
}
