
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers
{
    internal class RtmpChunkEventHandler : IRequestHandler<RtmpChunkEvent, RtmpEventConsumingResult>
    {
        private readonly IRtmpMessageDispatcher<IRtmpClientSessionContext> _dispatcher;
        private readonly IRtmpChunkMessageAggregatorService _chunkMessageAggregator;
        private readonly IRtmpAcknowledgementHandlerService _acknowledgementHandler;
        private readonly ILogger _logger;

        public RtmpChunkEventHandler(
            IRtmpMessageDispatcher<IRtmpClientSessionContext> dispatcher,
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
                @event.NetworkStream, @event.ClientContext, cancellationToken);

            if (aggregationResult.IsComplete && !await HandleRtmpMessageAsync(@event, aggregationResult, cancellationToken))
            {
                return new RtmpEventConsumingResult(false, aggregationResult.ChunkMessageSize);
            }

            _acknowledgementHandler.Handle(@event.ClientContext, aggregationResult.ChunkMessageSize);

            return new RtmpEventConsumingResult(true, aggregationResult.ChunkMessageSize);
        }

        private async Task<bool> HandleRtmpMessageAsync(
            RtmpChunkEvent @event, RtmpChunkMessageAggregationResult aggregationResult, CancellationToken cancellationToken)
        {
            try
            {
                return await DispatchRtmpMessageAsync(aggregationResult.ChunkStreamContext, @event.ClientContext, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.FailedToHandleRtmpMessage(@event.ClientContext.Client.Id, ex);
                return false;
            }
            finally
            {
                _chunkMessageAggregator.ResetChunkStreamContext(aggregationResult.ChunkStreamContext);
            }
        }

        private async ValueTask<bool> DispatchRtmpMessageAsync(
            IRtmpChunkStreamContext chunkStreamContext, IRtmpClientSessionContext clientContext, CancellationToken cancellationToken)
        {
            Debug.Assert(chunkStreamContext.PayloadBuffer != null);
            return await _dispatcher.DispatchAsync(chunkStreamContext, clientContext, cancellationToken);
        }
    }
}
