using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
{
    internal class RtmpClientServerEventHandler : IRtmpServerEventHandler
    {
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;
        private readonly IRtmpStreamDeletionService _streamDeletionService;

        public RtmpClientServerEventHandler(
            IRtmpMediaMessageManagerService mediaMessageManager,
            IRtmpStreamDeletionService streamDeletionService)
        {
            _mediaMessageManager = mediaMessageManager;
            _streamDeletionService = streamDeletionService;
        }

        public Task OnRtmpClientCreatedAsync(IRtmpClientContext clientContext)
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
    }
}
