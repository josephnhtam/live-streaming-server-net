using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpServerEventHandlers
{
    internal class RtmpClientPeerServerEventHandler : IRtmpInternalEventHandler
    {
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;
        private readonly IRtmpStreamDeletionService _streamDeletionService;

        public RtmpClientPeerServerEventHandler(
            IRtmpMediaMessageManagerService mediaMessageManager,
            IRtmpStreamDeletionService streamDeletionService)
        {
            _mediaMessageManager = mediaMessageManager;
            _streamDeletionService = streamDeletionService;
        }

        public Task OnRtmpClientCreatedAsync(IRtmpClientPeerContext peerContext)
        {
            _mediaMessageManager.RegisterClientPeer(peerContext);
            return Task.CompletedTask;
        }

        public async Task OnRtmpClientDisposedAsync(IRtmpClientPeerContext peerContext)
        {
            _mediaMessageManager.UnregisterClientPeer(peerContext);
            await _streamDeletionService.DeleteStream(peerContext);
        }
    }
}
