using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.Handshakes;
using LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Mediator;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers
{
    internal class RtmpHandshakeInitiationEventHandler : IRequestHandler<RtmpHandshakeInitiationEvent, bool>
    {
        private readonly IDataBufferPool _dataBufferPool;

        public RtmpHandshakeInitiationEventHandler(IDataBufferPool dataBufferPool)
        {
            _dataBufferPool = dataBufferPool;
        }

        public ValueTask<bool> Handle(RtmpHandshakeInitiationEvent @event, CancellationToken cancellationToken)
        {
            InitiateHandshake(@event.Context);
            return ValueTask.FromResult(true);
        }

        private void InitiateHandshake(IRtmpSessionContext context)
        {
            var outgoingBuffer = _dataBufferPool.Obtain();

            try
            {
                var handshake = new SimpleHandshake();
                handshake.WriteC0(outgoingBuffer);
                handshake.WriteC1(context, outgoingBuffer);

                context.Session.Send(outgoingBuffer);
            }
            finally
            {
                _dataBufferPool.Recycle(outgoingBuffer);
            }
        }
    }
}