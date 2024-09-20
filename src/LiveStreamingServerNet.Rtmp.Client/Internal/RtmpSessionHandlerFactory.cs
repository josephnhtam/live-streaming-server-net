using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpSessionHandlerFactory : ISessionHandlerFactory
    {
        private readonly IMediator _mediator;
        private readonly IRtmpSessionContextFactory _contextFactory;
        private readonly ILogger<RtmpSessionHandler> _logger;

        public RtmpSessionHandlerFactory(
            IMediator mediator,
            IRtmpSessionContextFactory contextFactory,
            ILogger<RtmpSessionHandler> logger)
        {
            _mediator = mediator;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public ISessionHandler Create(ISessionHandle session)
        {
            var context = _contextFactory.Create(session);
            return new RtmpSessionHandler(context, _mediator, _logger);
        }
    }
}