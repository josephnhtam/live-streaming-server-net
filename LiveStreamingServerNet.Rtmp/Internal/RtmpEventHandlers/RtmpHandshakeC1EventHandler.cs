﻿using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Handshakes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers
{
    internal class RtmpHandshakeC1EventHandler : IRequestHandler<RtmpHandshakeC1Event, bool>
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly ILogger _logger;

        public RtmpHandshakeC1EventHandler(INetBufferPool netBufferPool, ILogger<RtmpHandshakeC1EventHandler> logger)
        {
            _netBufferPool = netBufferPool;
            _logger = logger;
        }

        public async Task<bool> Handle(RtmpHandshakeC1Event @event, CancellationToken cancellationToken)
        {
            using var incomingBuffer = _netBufferPool.Obtain();
            await incomingBuffer.CopyStreamData(@event.NetworkStream, 1536, cancellationToken);

            using var outgoingBuffer = _netBufferPool.Obtain();
            if (HandleHandshake(@event, incomingBuffer, outgoingBuffer))
            {
                @event.ClientContext.State = RtmpClientState.HandshakeC2;
                @event.ClientContext.Client.Send(outgoingBuffer);

                _logger.HandshakeC1Handled(@event.ClientContext.Client.ClientId);

                return true;
            }

            _logger.HandshakeC1HandlingFailed(@event.ClientContext.Client.ClientId);

            return false;
        }

        private bool HandleHandshake(RtmpHandshakeC1Event @event, INetBuffer incomingBuffer, INetBuffer outgoingBuffer)
        {
            var clientContext = @event.ClientContext;
            var client = clientContext.Client;

            var complexHandshake0 = new ComplexHandshake(incomingBuffer, ComplexHandshakeType.Schema0);
            if (complexHandshake0.ValidateC1())
            {
                clientContext.HandshakeType = HandshakeType.ComplexHandshake0;
                complexHandshake0.WriteS0S1S2(outgoingBuffer);
                _logger.HandshakeType(client.ClientId, nameof(HandshakeType.ComplexHandshake0));
                return true;
            }

            var complexHandshake1 = new ComplexHandshake(incomingBuffer, ComplexHandshakeType.Schema1);
            if (complexHandshake1.ValidateC1())
            {
                clientContext.HandshakeType = HandshakeType.ComplexHandshake1;
                complexHandshake1.WriteS0S1S2(outgoingBuffer);
                _logger.HandshakeType(client.ClientId, nameof(HandshakeType.ComplexHandshake1));
                return true;
            }

            var simpleHandshake = new SimpleHandshake(incomingBuffer);
            if (simpleHandshake.ValidateC1())
            {
                clientContext.HandshakeType = HandshakeType.SimpleHandshake;
                simpleHandshake.WriteS0S1S2(outgoingBuffer);
                _logger.HandshakeType(client.ClientId, nameof(HandshakeType.SimpleHandshake));
                return true;
            }

            return false;
        }
    }
}
