using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Exceptions;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Logging;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Logging;
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

        public void Start(INetworkStreamWriter networkStream, CancellationToken cancellationToken)
        {
            _task = Task.Run(() => SendOutstandingBuffersAsync(networkStream, cancellationToken), cancellationToken);
        }

        public void Send(INetBuffer netBuffer, Action<bool>? callback)
        {
            var rentedBuffer = new RentedBuffer(netBuffer.Size);

            try
            {
                var originalPosition = netBuffer.Position;
                netBuffer.MoveTo(0).ReadBytes(rentedBuffer.Buffer, 0, rentedBuffer.Size);
                netBuffer.MoveTo(originalPosition);

                if (!_pendingMessageChannel.Writer.TryWrite(new PendingMessage(rentedBuffer, callback)))
                {
                    throw new Exception("Failed to write to the send channel");
                }
            }
            catch (Exception)
            {
                rentedBuffer.Unclaim();
                throw;
            }
        }

        public void Send(IRentedBuffer rentedBuffer, Action<bool>? callback)
        {
            rentedBuffer.Claim();

            try
            {
                if (!_pendingMessageChannel.Writer.TryWrite(new PendingMessage(rentedBuffer, callback)))
                {
                    throw new Exception("Failed to write to the send channel");
                }
            }
            catch (Exception)
            {
                rentedBuffer.Unclaim();
                throw;
            }
        }

        public void Send(Action<INetBuffer> writer, Action<bool>? callback)
        {
            using var netBuffer = ObtainNetBuffer();
            writer.Invoke(netBuffer);

            var rentedBuffer = new RentedBuffer(netBuffer.Size);

            try
            {
                netBuffer.MoveTo(0).ReadBytes(rentedBuffer.Buffer, 0, rentedBuffer.Size);

                if (!_pendingMessageChannel.Writer.TryWrite(new PendingMessage(rentedBuffer, callback)))
                {
                    throw new Exception("Failed to write to the send channel");
                }
            }
            catch (Exception)
            {
                rentedBuffer.Unclaim();
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

        public Task SendAsync(IRentedBuffer rentedBuffer)
        {
            var tcs = new TaskCompletionSource();
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

        private async Task SendOutstandingBuffersAsync(INetworkStreamWriter networkStream, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var (rentedBuffer, callback) = await _pendingMessageChannel.Reader.ReadAsync(cancellationToken);

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
                        _logger.SendDataError(_clientId, ex);
                    }
                    finally
                    {
                        rentedBuffer.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }

            while (_pendingMessageChannel.Reader.TryRead(out var pendingMessage))
            {
                InvokeCallback(pendingMessage.Callback, false);
                pendingMessage.RentedBuffer.Unclaim();
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
