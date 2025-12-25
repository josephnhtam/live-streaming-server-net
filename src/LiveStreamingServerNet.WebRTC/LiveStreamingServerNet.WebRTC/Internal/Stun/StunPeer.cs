using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Configurations;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;
using System.Collections.Concurrent;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun
{
    internal class StunPeer : IStunPeer, IDisposable
    {
        private readonly IStunSender _sender;
        private readonly StunClientConfiguration _config;
        private readonly IStunMessageHandler? _messageHandler;
        private readonly IDataBufferPool _bufferPool;
        private readonly ConcurrentDictionary<TransactionId, TaskCompletionSource<(StunMessage, UnknownAttributes?)>> _pendingTransactions;

        public StunPeer(
            IStunSender sender,
            StunClientConfiguration config,
            IDataBufferPool? bufferPool = null) : this(sender, config, null, bufferPool)
        {
        }

        public StunPeer(
            IStunSender sender,
            StunClientConfiguration config,
            IStunMessageHandler? messageHandler,
            IDataBufferPool? bufferPool = null)
        {
            _sender = sender;
            _config = config;
            _messageHandler = messageHandler;
            _bufferPool = bufferPool ?? DataBufferPool.Shared;
            _pendingTransactions = new ConcurrentDictionary<TransactionId, TaskCompletionSource<(StunMessage, UnknownAttributes?)>>();
        }

        public async Task<(StunMessage, UnknownAttributes?)> SendRequestAsync(
            ushort method,
            IList<IStunAttribute> attributes,
            IPEndPoint remoteEndPoint,
            CancellationToken cancellation = default)
        {
            using var request = new StunMessage(method, StunClass.Request, attributes);

            using var buffer = _bufferPool.Obtain();
            request.Write(buffer);

            var tcs = new TaskCompletionSource<(StunMessage, UnknownAttributes?)>();
            if (!_pendingTransactions.TryAdd(request.TransactionId, tcs))
            {
                throw new InvalidOperationException("Transaction ID collision.");
            }

            try
            {
                return await SendBufferWithRetransmissionAsync(buffer, remoteEndPoint, tcs, cancellation);
            }
            finally
            {
                _pendingTransactions.TryRemove(request.TransactionId, out _);
            }
        }

        public async Task SendIndicationAsync(
            ushort method,
            IList<IStunAttribute> attributes,
            IPEndPoint remoteEndPoint,
            CancellationToken cancellation = default)
        {
            using var indication = new StunMessage(method, StunClass.Indication, attributes);

            using var buffer = _bufferPool.Obtain();
            indication.Write(buffer);

            await _sender.SendAsync(buffer.AsSpan(), remoteEndPoint, cancellation);
        }

        public async ValueTask FeedPacketAsync(
            IDataBuffer buffer,
            IPEndPoint remoteEndPoint,
            CancellationToken cancellation = default)
        {
            var (message, unknownAttributes) = StunMessage.Read(buffer);

            switch (message.Class)
            {
                case StunClass.Request when _messageHandler != null:
                    await HandleRequestAsync(message, unknownAttributes, remoteEndPoint, cancellation);
                    return;

                case StunClass.Indication when _messageHandler != null:
                    await HandleIndicationAsync(message, unknownAttributes, remoteEndPoint, cancellation);
                    return;

                case StunClass.SuccessResponse or StunClass.ErrorResponse:
                    if (_pendingTransactions.TryGetValue(message.TransactionId, out var tcs))
                    {
                        tcs.TrySetResult((message, unknownAttributes));
                        return;
                    }
                    else
                    {
                        message.Dispose();
                        return;
                    }

                default:
                    message.Dispose();
                    return;
            }
        }

        private async ValueTask HandleRequestAsync(
            StunMessage message,
            UnknownAttributes? unknownAttributes,
            IPEndPoint remoteEndPoint,
            CancellationToken cancellation)
        {
            using var _ = message;

            try
            {
                if (_messageHandler == null)
                {
                    return;
                }

                using var response = await CreateResponseAsync();

                if (response.Class is not (StunClass.SuccessResponse or StunClass.ErrorResponse))
                {
                    throw new InvalidOperationException(
                        "STUN response message must be either Success Response or Error Response.");
                }

                using var buffer = _bufferPool.Obtain();
                response.Write(buffer);

                await _sender.SendAsync(buffer.AsSpan(), remoteEndPoint, cancellation);
            }
            catch (Exception)
            {
                // todo: add logs
            }

            return;

            async ValueTask<StunMessage> CreateResponseAsync()
            {
                if (_config.HandleUnknownComprehensionRequiredAttributes &&
                    unknownAttributes?.HasUnknownComprehensionRequiredAttributes() == true)
                {
                    return new StunMessage(
                        message.TransactionId,
                        message.Method,
                        StunClass.ErrorResponse,
                        [
                            new ErrorCodeAttribute(420, "Unknown Comprehension-Required Attribute"),
                            new UnknownAttributesAttribute(
                                unknownAttributes.UnknownComprehensionRequiredAttributes().ToList())
                        ]);
                }

                return await _messageHandler.HandleRequestAsync(message, unknownAttributes, remoteEndPoint, cancellation);
            }
        }

        private async ValueTask HandleIndicationAsync(
            StunMessage message,
            UnknownAttributes? unknownAttributes,
            IPEndPoint remoteEndPoint,
            CancellationToken cancellation)
        {
            using var _ = message;

            try
            {
                if (_messageHandler == null)
                {
                    return;
                }

                await _messageHandler.HandleIndicationAsync(
                    message, unknownAttributes, remoteEndPoint, cancellation);
            }
            catch (Exception)
            {
                // todo: add logs
            }
        }

        private async Task<(StunMessage, UnknownAttributes?)> SendBufferWithRetransmissionAsync(
            IDataBuffer buffer,
            IPEndPoint remoteEndPoint,
            TaskCompletionSource<(StunMessage, UnknownAttributes?)> tcs,
            CancellationToken cancellation)
        {
            for (var retries = 0; retries <= _config.MaxRetransmissions; retries++)
            {
                cancellation.ThrowIfCancellationRequested();

                await _sender.SendAsync(buffer.AsSpan(), remoteEndPoint, cancellation);

                var timeout = Math.Min(
                    _config.RetransmissionTimeout * (int)Math.Pow(2, retries),
                    _config.MaxRetransmissionTimeout);

                var timeoutTask = Task.Delay(timeout, cancellation);
                var resultTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (resultTask == tcs.Task)
                {
                    return await tcs.Task;
                }
            }

            var transactionTimeout = _config.RetransmissionTimeout * _config.TransactionTimeoutFactor;
            var transactionTimeoutTask = Task.Delay(transactionTimeout, cancellation);
            if (await Task.WhenAny(tcs.Task, transactionTimeoutTask) == tcs.Task)
            {
                return await tcs.Task;
            }

            throw new TimeoutException($"STUN transaction timed out after {transactionTimeout} ms.");
        }

        public void Dispose()
        {
            foreach (var pendingTransaction in _pendingTransactions.Values)
            {
                pendingTransaction.TrySetCanceled();
            }

            _pendingTransactions.Clear();
        }
    }
}
