using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.RateLimiting.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpClientHandlerFactory : IClientHandlerFactory
    {
        private readonly IMediator _mediator;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly ILogger<RtmpClientHandler> _logger;
        private readonly IBandwidthLimiterFactory? _bandwidthLimiterFactory;

        public RtmpClientHandlerFactory(
            IMediator mediator,
            IRtmpServerConnectionEventDispatcher eventDispatcher,
            ILogger<RtmpClientHandler> logger,
            IBandwidthLimiterFactory? bandwidthLimiterFactory = null)
        {
            _mediator = mediator;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
            _bandwidthLimiterFactory = bandwidthLimiterFactory;
        }

        public IClientHandler CreateClientHandler()
        {
            return new RtmpClientHandler(_mediator, _eventDispatcher, _logger, _bandwidthLimiterFactory);
        }
    }
}
