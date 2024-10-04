using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers
{
    internal class RtmpChunkEventHandler : IRequestHandler<RtmpChunkEvent, RtmpEventConsumingResult>
    {
        private readonly IRtmpMessageDispatcher<IRtmpSessionContext> _dispatcher;
        private readonly IRtmpChunkMessageAggregatorService _chunkMessageAggregator;
        private readonly IRtmpAcknowledgementHandlerService _acknowledgementHandler;
        private readonly ILogger _logger;

        public RtmpChunkEventHandler(
            IRtmpMessageDispatcher<IRtmpSessionContext> dispatcher,
            IRtmpChunkMessageAggregatorService chunkMessageAggregator,
            IRtmpAcknowledgementHandlerService acknowledgementHandler,
            ILogger<RtmpChunkEventHandler> logger)
        {
            _dispatcher = dispatcher;
            _chunkMessageAggregator = chunkMessageAggregator;
            _acknowledgementHandler = acknowledgementHandler;
            _logger = logger;
        }

        public async ValueTask<RtmpEventConsumingResult> Handle(RtmpChunkEvent @event, CancellationToken cancellationToken)
        {
            var aggregationResult = await _chunkMessageAggregator.AggregateChunkMessagesAsync(
                @event.NetworkStream, @event.Context, cancellationToken);

            if (aggregationResult.IsComplete && !await HandleRtmpMessageAsync(@event, aggregationResult, cancellationToken))
            {
                return new RtmpEventConsumingResult(false, aggregationResult.ChunkMessageSize);
            }

            _acknowledgementHandler.Handle(@event.Context, aggregationResult.ChunkMessageSize);

            return new RtmpEventConsumingResult(true, aggregationResult.ChunkMessageSize);
        }

        private async Task<bool> HandleRtmpMessageAsync(
            RtmpChunkEvent @event, RtmpChunkMessageAggregationResult aggregationResult, CancellationToken cancellationToken)
        {
            try
            {
                return await DispatchRtmpMessageAsync(aggregationResult.ChunkStreamContext, @event.Context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.FailedToHandleRtmpMessage(@event.Context.Session.Id, ex);
                return false;
            }
            finally
            {
                _chunkMessageAggregator.ResetChunkStreamContext(aggregationResult.ChunkStreamContext);
            }
        }

        private async ValueTask<bool> DispatchRtmpMessageAsync(
            IRtmpChunkStreamContext chunkStreamContext, IRtmpSessionContext clientContext, CancellationToken cancellationToken)
        {
            Debug.Assert(chunkStreamContext.PayloadBuffer != null);
            return await _dispatcher.DispatchAsync(chunkStreamContext, clientContext, cancellationToken);
        }
    }
}
