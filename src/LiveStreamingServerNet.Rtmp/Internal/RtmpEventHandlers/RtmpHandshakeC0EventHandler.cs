using LiveStreamingServerNet.Rtmp.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers
{
    internal class RtmpHandshakeC0EventHandler : IRequestHandler<RtmpHandshakeC0Event, bool>
    {
        private readonly ILogger _logger;

        public RtmpHandshakeC0EventHandler(ILogger<RtmpHandshakeC0EventHandler> logger)
        {
            _logger = logger;
        }

        public async Task<bool> Handle(RtmpHandshakeC0Event @event, CancellationToken cancellationToken)
        {
            var payload = new byte[1];
            await @event.NetworkStream.ReadExactlyAsync(payload, 0, 1, cancellationToken);

            @event.ClientContext.State = RtmpClientState.HandshakeC1;

            _logger.HandshakeC0Handled(@event.ClientContext.Client.ClientId);

            return true;
        }
    }
}
