using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.RateLimiting.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpClientHandlerFactory : IClientHandlerFactory
    {
        private readonly IMediator _mediator;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly IRtmpClientContextFactory _clientContextFactory;
        private readonly ILogger<RtmpClientHandler> _logger;
        private readonly IBandwidthLimiterFactory? _bandwidthLimiterFactory;

        public RtmpClientHandlerFactory(
            IMediator mediator,
            IRtmpServerConnectionEventDispatcher eventDispatcher,
            IRtmpClientContextFactory clientContextFactory,
            ILogger<RtmpClientHandler> logger,
            IBandwidthLimiterFactory? bandwidthLimiterFactory = null)
        {
            _mediator = mediator;
            _eventDispatcher = eventDispatcher;
            _clientContextFactory = clientContextFactory;
            _logger = logger;
            _bandwidthLimiterFactory = bandwidthLimiterFactory;
        }

        public IClientHandler CreateClientHandler()
        {
            return new RtmpClientHandler(_mediator, _eventDispatcher, _clientContextFactory, _logger, _bandwidthLimiterFactory);
        }
    }
}
