using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEvents;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers
{
    internal class RtmpHandshakeC0EventHandler : IRequestHandler<RtmpHandshakeC0Event, RtmpEventConsumingResult>
    {
        private readonly ILogger _logger;

        public RtmpHandshakeC0EventHandler(ILogger<RtmpHandshakeC0EventHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<RtmpEventConsumingResult> Handle(RtmpHandshakeC0Event @event, CancellationToken cancellationToken)
        {
            var payload = new byte[1];
            await @event.NetworkStream.ReadExactlyAsync(payload, 0, 1, cancellationToken);

            @event.ClientContext.State = RtmpClientSessionState.HandshakeC1;
            _logger.HandshakeC0Handled(@event.ClientContext.Client.Id);

            return new RtmpEventConsumingResult(true, 1);
        }
    }
}
