using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
{
    internal class RtmpClientPeerServerEventHandler : IRtmpServerEventHandler
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

        public Task OnRtmpClientDisposedAsync(IRtmpClientPeerContext peerContext)
        {
            _mediaMessageManager.UnregisterClientPeer(peerContext);
            _streamDeletionService.DeleteStream(peerContext);
            return Task.CompletedTask;
        }
    }
}
