using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.RateLimiting.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal class RtmpClientSessionHandlerFactory : ISessionHandlerFactory
    {
        private readonly IMediator _mediator;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly IRtmpClientSessionContextFactory _clientContextFactory;
        private readonly ILogger<RtmpClientSessionHandler> _logger;
        private readonly IBandwidthLimiterFactory? _bandwidthLimiterFactory;

        public RtmpClientSessionHandlerFactory(
            IMediator mediator,
            IDataBufferPool dataBufferPool,
            IRtmpServerConnectionEventDispatcher eventDispatcher,
            IRtmpClientSessionContextFactory clientContextFactory,
            ILogger<RtmpClientSessionHandler> logger,
            IBandwidthLimiterFactory? bandwidthLimiterFactory = null)
        {
            _mediator = mediator;
            _dataBufferPool = dataBufferPool;
            _eventDispatcher = eventDispatcher;
            _clientContextFactory = clientContextFactory;
            _logger = logger;
            _bandwidthLimiterFactory = bandwidthLimiterFactory;
        }

        public ISessionHandler Create(ISessionHandle client)
        {
            var clientContext = _clientContextFactory.Create(client);
            return new RtmpClientSessionHandler(clientContext, _mediator, _dataBufferPool, _eventDispatcher, _logger, _bandwidthLimiterFactory);
        }
    }
}
