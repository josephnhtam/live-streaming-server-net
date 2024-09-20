using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Internal;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers
{
    internal class RtmpHandshakeS0EventHandler : IRequestHandler<RtmpHandshakeS0Event, RtmpEventConsumingResult>
    {
        private readonly ILogger _logger;

        public RtmpHandshakeS0EventHandler(ILogger<RtmpHandshakeS0EventHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<RtmpEventConsumingResult> Handle(RtmpHandshakeS0Event @event, CancellationToken cancellationToken)
        {
            var payload = new byte[1];
            await @event.NetworkStream.ReadExactlyAsync(payload, 0, 1, cancellationToken);

            @event.Context.State = RtmpSessionState.HandshakeS1;
            _logger.HandshakeS0Handled(@event.Context.Session.Id);

            return new RtmpEventConsumingResult(true, 1);
        }
    }
}