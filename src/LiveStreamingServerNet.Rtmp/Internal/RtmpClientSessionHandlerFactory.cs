using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.RateLimiting.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpClientSessionHandlerFactory : ISessionHandlerFactory
    {
        private readonly IMediator _mediator;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly IRtmpClientSessionContextFactory _clientContextFactory;
        private readonly ILogger<RtmpClientSessionHandler> _logger;
        private readonly IBandwidthLimiterFactory? _bandwidthLimiterFactory;

        public RtmpClientSessionHandlerFactory(
            IMediator mediator,
            IRtmpServerConnectionEventDispatcher eventDispatcher,
            IRtmpClientSessionContextFactory clientContextFactory,
            ILogger<RtmpClientSessionHandler> logger,
            IBandwidthLimiterFactory? bandwidthLimiterFactory = null)
        {
            _mediator = mediator;
            _eventDispatcher = eventDispatcher;
            _clientContextFactory = clientContextFactory;
            _logger = logger;
            _bandwidthLimiterFactory = bandwidthLimiterFactory;
        }

        public ISessionHandler Create(ISessionHandle client)
        {
            var clientContext = _clientContextFactory.Create(client);
            return new RtmpClientSessionHandler(clientContext, _mediator, _eventDispatcher, _logger, _bandwidthLimiterFactory);
        }
    }
}
