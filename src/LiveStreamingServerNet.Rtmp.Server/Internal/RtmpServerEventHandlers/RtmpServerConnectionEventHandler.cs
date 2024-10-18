using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpServerEventHandlers
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

        public ValueTask OnRtmpClientConnectedAsync(IEventContext context, IRtmpClientSessionContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            _mediaMessageBroadcaster.RegisterClient(clientContext);
            return ValueTask.CompletedTask;
        }

        public async ValueTask OnRtmpClientDisposingAsync(IEventContext context, IRtmpClientSessionContext clientContext)
        {
            await _mediaMessageBroadcaster.UnregisterClientAsync(clientContext);

            foreach (var streamContext in clientContext.GetStreamContexts())
                await _streamDeletionService.DeleteStreamAsync(streamContext);
        }

        public ValueTask OnRtmpClientDisposedAsync(IEventContext context, IRtmpClientSessionContext clientContext)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpClientCreatedAsync(IEventContext context, IRtmpClientSessionContext clientContext)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpClientHandshakeCompleteAsync(IEventContext context, IRtmpClientSessionContext clientContext)
        {
            return ValueTask.CompletedTask;
        }
    }
}
