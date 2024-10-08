using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Exceptions;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class BufferSender : IBufferSender
    {
        private readonly IDataBufferPool _dataBufferPool;
        private readonly Channel<PendingBuffer> _pendingBufferChannel;
        private readonly ILogger _logger;

        private INetworkStreamWriter? _networkStream;
        private Task? _task;

        public BufferSender(IDataBufferPool dataBufferPool, ILogger<BufferSender> logger)
        {
            _dataBufferPool = dataBufferPool;
            _logger = logger;

            _pendingBufferChannel = Channel.CreateUnbounded<PendingBuffer>(
                new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = true });
        }

        public void Start(INetworkStreamWriter networkStream, CancellationToken cancellationToken)
        {
            _networkStream = networkStream;
            _task = Task.Run(() => SendOutstandingBuffersAsync(networkStream, cancellationToken), cancellationToken);
        }

        public void Send(IDataBuffer dataBuffer, Action<bool>? callback)
        {
            var rentedBuffer = dataBuffer.ToRentedBuffer();

            try
            {
                if (!_pendingBufferChannel.Writer.TryWrite(new PendingBuffer(rentedBuffer, callback)))
                {
                    throw new Exception("Failed to write to the send channel");
                }
            }
            catch (Exception ex)
            {
                _logger.BufferWritingError(ex);
                rentedBuffer.Unclaim();
                callback?.Invoke(false);
            }
        }

        public void Send(IRentedBuffer rentedBuffer, Action<bool>? callback)
        {
            rentedBuffer.Claim();

            try
            {
                if (!_pendingBufferChannel.Writer.TryWrite(new PendingBuffer(rentedBuffer, callback)))
                {
                    throw new Exception("Failed to write to the send channel");
                }
            }
            catch (Exception ex)
            {
                _logger.BufferWritingError(ex);
                rentedBuffer.Unclaim();
                callback?.Invoke(false);
            }
        }

        public void Send(Action<IDataBuffer> writer, Action<bool>? callback)
        {
            var dataBuffer = _dataBufferPool.Obtain();

            try
            {
                writer.Invoke(dataBuffer);

                var rentedBuffer = dataBuffer.ToRentedBuffer();

                try
                {
                    if (!_pendingBufferChannel.Writer.TryWrite(new PendingBuffer(rentedBuffer, callback)))
                    {
                        throw new Exception("Failed to write to the send channel");
                    }
                }
                catch (Exception ex)
                {
                    _logger.BufferWritingError(ex);
                    rentedBuffer.Unclaim();
                    callback?.Invoke(false);
                }
            }
            finally
            {
                _dataBufferPool.Recycle(dataBuffer);
            }
        }

        public ValueTask SendAsync(IDataBuffer dataBuffer)
        {
            var tcs = new ValueTaskCompletionSource();
            Send(dataBuffer, SetResult);
            return tcs.Task;

            void SetResult(bool successful)
            {
                if (successful)
                    tcs.SetResult();
                else
                    tcs.SetException(new BufferSendingException());
            }
        }

        public ValueTask SendAsync(IRentedBuffer rentedBuffer)
        {
            var tcs = new ValueTaskCompletionSource();
            Send(rentedBuffer, SetResult);
            return tcs.Task;

            void SetResult(bool successful)
            {
                if (successful)
                    tcs.SetResult();
                else
                    tcs.SetException(new BufferSendingException());
            }
        }

        public ValueTask SendAsync(Action<IDataBuffer> writer)
        {
            var tcs = new ValueTaskCompletionSource();
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

        private async Task SendOutstandingBuffersAsync(INetworkStreamWriter networkStream, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var (rentedBuffer, callback) = await _pendingBufferChannel.Reader.ReadAsync(cancellationToken);

                    try
                    {
                        await networkStream.WriteAsync(rentedBuffer.Buffer, 0, rentedBuffer.Size, cancellationToken);
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
                        _logger.SendDataError(networkStream, ex);
                    }
                    finally
                    {
                        rentedBuffer.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            finally
            {
                _pendingBufferChannel.Writer.Complete();

                while (_pendingBufferChannel.Reader.TryRead(out var pendingMessage))
                {
                    InvokeCallback(pendingMessage.Callback, false);
                    pendingMessage.RentedBuffer.Unclaim();
                }
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
                if (_networkStream != null)
                {
                    _logger.OutstandingBufferSenderDisposeError(_networkStream, ex);
                }
            }
        }
    }
}
