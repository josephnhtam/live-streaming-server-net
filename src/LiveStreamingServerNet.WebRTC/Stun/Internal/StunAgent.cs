using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Stun.Configurations;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Channels;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal
{
    using TransactionDictionary =
        ConcurrentDictionary<TransactionId, TaskCompletionSource<StunResponse>>;

    internal class StunAgent : IStunAgent
    {
        private readonly IStunSender _sender;
        private readonly StunAgentConfiguration _config;
        private readonly IDataBufferPool _bufferPool;

        private readonly TransactionDictionary _pendingTransactions;

        private readonly Channel<IncomingStunMessage> _messageChannel;
        private readonly CancellationTokenSource _cts;
        private readonly Task _loopTask;

        private IStunMessageHandler? _messageHandler;
        private volatile int _isDisposed;

        public StunAgent(
            IStunSender sender,
            StunAgentConfiguration config,
            IDataBufferPool? bufferPool = null)
        {
            _sender = sender;
            _config = config;
            _bufferPool = bufferPool ?? DataBufferPool.Shared;
            _pendingTransactions = new TransactionDictionary();

            _messageChannel = Channel.CreateUnbounded<IncomingStunMessage>(new()
            {
                AllowSynchronousContinuations = true,
                SingleReader = true
            });

            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => IncomingMessageLoopAsync(_cts.Token));
        }

        public async Task<StunResponse> SendRequestAsync(
            StunMessage request,
            IPEndPoint remoteEndPoint,
            CancellationToken cancellation = default)
        {
            if (_isDisposed == 1)
            {
                throw new ObjectDisposedException(nameof(StunAgent));
            }

            if (request.Class != StunClass.Request)
            {
                throw new ArgumentException("STUN message must be of Request class.", nameof(request));
            }

            using var buffer = _bufferPool.Obtain();
            request.Write(buffer);

            var tcs = new TaskCompletionSource<StunResponse>();
            var transactionId = request.TransactionId;

            if (!_pendingTransactions.TryAdd(transactionId, tcs))
            {
                throw new InvalidOperationException("Transaction ID collision.");
            }

            try
            {
                transactionId.Claim();

                var result = await SendBufferWithRetransmissionAsync(
                    buffer,
                    remoteEndPoint,
                    tcs,
                    cancellation).ConfigureAwait(false);

                try
                {
                    cancellation.ThrowIfCancellationRequested();
                    return result;
                }
                catch (OperationCanceledException)
                {
                    result.Dispose();
                    throw;
                }
            }
            finally
            {
                _pendingTransactions.TryRemove(transactionId, out _);
                transactionId.Unclaim();
            }
        }

        public void SetMessageHandler(IStunMessageHandler? handler)
        {
            _messageHandler = handler;
        }

        public async ValueTask SendIndicationAsync(
            StunMessage indication,
            IPEndPoint remoteEndPoint,
            CancellationToken cancellation = default)
        {
            if (_isDisposed == 1)
            {
                throw new ObjectDisposedException(nameof(StunAgent));
            }

            if (indication.Class != StunClass.Indication)
            {
                throw new ArgumentException(
                    "STUN message must be of Indication class.",
                    nameof(indication));
            }

            using var buffer = _bufferPool.Obtain();
            indication.Write(buffer);

            await _sender.SendAsync(buffer, remoteEndPoint, cancellation).ConfigureAwait(false);
        }

        public void FeedPacket(IDataBufferReader buffer, IPEndPoint remoteEndPoint, object? state = null)
        {
            if (_isDisposed == 1)
                return;

            var (message, unknownAttributes) = StunMessage.Read(buffer);
            var messageHandler = _messageHandler;

            switch (message.Class)
            {
                case StunClass.Request or StunClass.Indication when messageHandler != null:
                    if (!_messageChannel.Writer.TryWrite(new(message, unknownAttributes, remoteEndPoint, state)))
                    {
                        message.Dispose();
                    }

                    return;

                case StunClass.SuccessResponse or StunClass.ErrorResponse:
                    if (_pendingTransactions.TryGetValue(message.TransactionId, out var tcs))
                    {
                        tcs.TrySetResult(new StunResponse(message, unknownAttributes, remoteEndPoint, state));
                        return;
                    }

                    message.Dispose();
                    return;

                default:
                    message.Dispose();
                    return;
            }
        }

        private async ValueTask HandleRequestAsync(
            IStunMessageHandler messageHandler,
            StunMessage message,
            UnknownAttributes? unknownAttributes,
            IPEndPoint remoteEndPoint,
            object? state,
            CancellationToken cancellation)
        {
            using var _ = message;

            try
            {
                using var response = await CreateResponseAsync().ConfigureAwait(false);

                if (response == null)
                {
                    return;
                }

                if (response.TransactionId != message.TransactionId)
                {
                    throw new InvalidOperationException(
                        "STUN response message must have the same Transaction ID as the request.");
                }

                if (response.Class is not (StunClass.SuccessResponse or StunClass.ErrorResponse))
                {
                    throw new InvalidOperationException(
                        "STUN response message must be either Success Response or Error Response.");
                }

                using var buffer = _bufferPool.Obtain();
                response.Write(buffer);

                await _sender.SendAsync(buffer, remoteEndPoint, cancellation).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // todo: add logs
            }

            return;

            async ValueTask<StunMessage?> CreateResponseAsync()
            {
                if (_config.HandleUnknownComprehensionRequiredAttributes &&
                    unknownAttributes?.HasUnknownComprehensionRequiredAttributes() == true)
                {
                    return new StunMessage(
                        message.TransactionId,
                        StunClass.ErrorResponse,
                        message.Method,
                        [
                            new ErrorCodeAttribute(420, "Unknown Comprehension-Required Attribute"),
                            new UnknownAttributesAttribute(
                                unknownAttributes.UnknownComprehensionRequiredAttributes().ToList())
                        ]);
                }

                return await messageHandler.HandleRequestAsync(
                    message,
                    unknownAttributes,
                    remoteEndPoint,
                    state,
                    cancellation).ConfigureAwait(false);
            }
        }

        private async ValueTask HandleIndicationAsync(
            IStunMessageHandler messageHandler,
            StunMessage message,
            UnknownAttributes? unknownAttributes,
            IPEndPoint remoteEndPoint,
            object? state,
            CancellationToken cancellation)
        {
            using var _ = message;

            try
            {
                await messageHandler.HandleIndicationAsync(
                    message,
                    unknownAttributes,
                    remoteEndPoint,
                    state,
                    cancellation).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // todo: add logs
            }
        }

        private async Task<StunResponse> SendBufferWithRetransmissionAsync(
            IDataBuffer buffer,
            IPEndPoint remoteEndPoint,
            TaskCompletionSource<StunResponse> tcs,
            CancellationToken cancellation)
        {
            for (var retries = 0; retries <= _config.MaxRetransmissions; retries++)
            {
                cancellation.ThrowIfCancellationRequested();

                await _sender.SendAsync(buffer, remoteEndPoint, cancellation).ConfigureAwait(false);

                var timeout = (int)Math.Min(
                    _config.RetransmissionTimeout.TotalMilliseconds * (int)Math.Pow(2, retries),
                    _config.MaxRetransmissionTimeout.TotalMilliseconds);

                var timeoutTask = Task.Delay(timeout, cancellation);
                var resultTask = await Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

                if (resultTask == tcs.Task)
                {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }

            var transactionTimeout =
                _config.RetransmissionTimeout * _config.TransactionTimeoutFactor;

            var transactionTimeoutTask = Task.Delay(transactionTimeout, cancellation);

            if (await Task.WhenAny(tcs.Task, transactionTimeoutTask).ConfigureAwait(false) == tcs.Task)
            {
                return await tcs.Task.ConfigureAwait(false);
            }

            throw new TimeoutException(
                $"STUN transaction timed out after {transactionTimeout} ms.");
        }

        private async Task IncomingMessageLoopAsync(CancellationToken cancellation)
        {
            try
            {
                await foreach (var incoming in _messageChannel.Reader.ReadAllAsync(cancellation).ConfigureAwait(false))
                {
                    var messageHandler = _messageHandler;
                    var (message, unknownAttributes, remoteEndPoint, state) = incoming;

                    try
                    {
                        switch (message.Class)
                        {
                            case StunClass.Request when messageHandler != null:
                                await HandleRequestAsync(
                                    messageHandler,
                                    message,
                                    unknownAttributes,
                                    remoteEndPoint,
                                    state,
                                    cancellation).ConfigureAwait(false);
                                break;

                            case StunClass.Indication when messageHandler != null:
                                await HandleIndicationAsync(
                                    messageHandler,
                                    message,
                                    unknownAttributes,
                                    remoteEndPoint,
                                    state,
                                    cancellation).ConfigureAwait(false);
                                break;

                            default:
                                message.Dispose();
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // todo: add logs
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ChannelClosedException) { }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            {
                return;
            }

            _cts.Cancel();

            await ErrorBoundary.ExecuteAsync(async () => await _loopTask.ConfigureAwait(false)).ConfigureAwait(false);

            _messageChannel.Writer.TryComplete();
            while (_messageChannel.Reader.TryRead(out var incomingMessage))
            {
                incomingMessage.Message.Dispose();
            }

            foreach (var pendingTransaction in _pendingTransactions.Values)
            {
                pendingTransaction.TrySetCanceled();
            }

            _pendingTransactions.Clear();

            _cts.Dispose();
        }

        private record struct IncomingStunMessage(
            StunMessage Message,
            UnknownAttributes? UnknownAttributes,
            IPEndPoint RemoteEndPoint,
            object? State);
    }
}
