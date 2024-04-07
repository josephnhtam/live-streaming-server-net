using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Exceptions;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Logging;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class NetBufferSender : INetBufferSender
    {
        private readonly uint _clientId;
        private readonly INetBufferPool _netBufferPool;
        private readonly Channel<PendingMessage> _pendingMessageChannel;
        private readonly ILogger _logger;

        private Task? _task;

        public NetBufferSender(uint clientId, INetBufferPool netBufferPool, ILogger<NetBufferSender> logger)
        {
            _clientId = clientId;
            _netBufferPool = netBufferPool;
            _logger = logger;

            _pendingMessageChannel = Channel.CreateUnbounded<PendingMessage>();
        }

        public void Start(Stream networkStream, CancellationToken cancellationToken)
        {
            _task = Task.Run(() => SendOutstandingBuffersAsync(networkStream, cancellationToken), cancellationToken);
        }

        public void Send(INetBuffer netBuffer, Action<bool>? callback)
        {
            var rentedBuffer = ArrayPool<byte>.Shared.Rent(netBuffer.Size);

            try
            {
                var originalPosition = netBuffer.Position;
                netBuffer.MoveTo(0).ReadBytes(rentedBuffer, 0, netBuffer.Size);
                netBuffer.MoveTo(originalPosition);

                if (!_pendingMessageChannel.Writer.TryWrite(new PendingMessage(rentedBuffer, netBuffer.Size, callback)))
                {
                    throw new Exception("Failed to write to the send channel");
                }
            }
            catch (Exception)
            {
                ArrayPool<byte>.Shared.Return(rentedBuffer);
                throw;
            }
        }

        public void Send(Action<INetBuffer> writer, Action<bool>? callback)
        {
            using var netBuffer = ObtainNetBuffer();
            writer.Invoke(netBuffer);

            var rentedBuffer = ArrayPool<byte>.Shared.Rent(netBuffer.Size);

            try
            {
                netBuffer.MoveTo(0).ReadBytes(rentedBuffer, 0, netBuffer.Size);

                if (!_pendingMessageChannel.Writer.TryWrite(new PendingMessage(rentedBuffer, netBuffer.Size, callback)))
                {
                    throw new Exception("Failed to write to the send channel");
                }
            }
            catch (Exception)
            {
                ArrayPool<byte>.Shared.Return(rentedBuffer);
                throw;
            }
        }

        public Task SendAsync(INetBuffer netBuffer)
        {
            var tcs = new TaskCompletionSource();
            Send(netBuffer, SetResult);
            return tcs.Task;

            void SetResult(bool successful)
            {
                if (successful)
                    tcs.SetResult();
                else
                    tcs.SetException(new BufferSendingException());
            }
        }

        public Task SendAsync(Action<INetBuffer> writer)
        {
            var tcs = new TaskCompletionSource();
            Send(writer, SetResult);
            return tcs.Task;

            void SetResult(bool successful)
            {
                if (successful)
                    tcs.SetResult();
                else
                    tcs.SetException(new BufferSendingException());
            }
        }

        private INetBuffer ObtainNetBuffer()
        {
            return _netBufferPool.Obtain();
        }

        private async Task SendOutstandingBuffersAsync(Stream networkStream, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var (rentedBuffer, bufferSize, callback) = await _pendingMessageChannel.Reader.ReadAsync(cancellationToken);

                    try
                    {
                        await networkStream.WriteAsync(rentedBuffer, 0, bufferSize, cancellationToken);
                        InvokeCallback(callback, true);
                    }
                    catch (Exception ex)
                    when (ex is IOException || (ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
                    {
                        InvokeCallback(callback, false);
                    }
                    catch (Exception ex)
                    {
                        InvokeCallback(callback, false);
                        _logger.SendDataError(_clientId, ex);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(rentedBuffer);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }

            while (_pendingMessageChannel.Reader.TryRead(out var pendingMessage))
            {
                InvokeCallback(pendingMessage.Callback, false);
                ArrayPool<byte>.Shared.Return(pendingMessage.RentedBuffer);
            }

            void InvokeCallback(Action<bool>? callback, bool successful)
            {
                try
                {
                    callback?.Invoke(successful);
                }
                catch { }
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_task != null)
                    await _task;
            }
            catch (Exception ex)
            {
                _logger.OutstandingBufferSenderDisposeError(_clientId, ex);
            }
        }
    }
}
