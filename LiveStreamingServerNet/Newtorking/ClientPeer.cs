using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Net.Sockets;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Newtorking
{
    public class ClientPeer : IClientPeer
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly ILogger? _logger;
        private readonly Channel<PendingMessage> _sendChannel;
        private TcpClient _tcpClient = default!;

        public uint PeerId { get; private set; }

        public ClientPeer(IServiceProvider services)
        {
            _netBufferPool = services.GetRequiredService<INetBufferPool>();
            _logger = services.GetRequiredService<ILogger<ClientPeer>>();
            _sendChannel = Channel.CreateUnbounded<PendingMessage>();
        }

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public void Initialize(uint peerId, TcpClient tcpClient)
        {
            PeerId = peerId;
            _tcpClient = tcpClient;
        }

        public async Task RunAsync(IClientPeerHandler handler, CancellationToken stoppingToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var cancellationToken = cts.Token;

            try
            {
                var networkStream = _tcpClient.GetStream();
                var readOnlyNetworkStream = new ReadOnlyNetworkStream(networkStream);

                var sendTask = SendOutstandingBuffersAsync(networkStream, cancellationToken);

                try
                {
                    while (_tcpClient.Connected && !cancellationToken.IsCancellationRequested)
                    {
                        if (!await handler.HandleClientPeerLoopAsync(readOnlyNetworkStream, cancellationToken))
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    cts.Cancel();
                }

                await sendTask;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (EndOfStreamException) { }
            catch (IOException) { }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred");
            }
            finally
            {
                _tcpClient.Close();
                handler.Dispose();
                _logger?.LogDebug("PeerId: {PeerId} | Disconnected", PeerId);
            }
        }

        private async Task SendOutstandingBuffersAsync(NetworkStream networkStream, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var (rentedBuffer, bufferSize, callback) = await _sendChannel.Reader.ReadAsync(cancellationToken);

                try
                {
                    await networkStream.WriteAsync(rentedBuffer, 0, bufferSize, cancellationToken);
                    callback?.Invoke();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "An error occurred while sending data to the client");
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rentedBuffer);
                }
            }
        }

        public void Send(INetBuffer netBuffer, Action? callback)
        {
            if (!IsConnected) return;

            var rentedBuffer = ArrayPool<byte>.Shared.Rent(netBuffer.Size);

            try
            {
                var originalPosition = netBuffer.Position;
                netBuffer.MoveTo(0).ReadBytes(rentedBuffer, 0, netBuffer.Size);
                netBuffer.MoveTo(originalPosition);

                if (!_sendChannel.Writer.TryWrite(new PendingMessage(rentedBuffer, netBuffer.Size, callback)))
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

        public void Send(Action<INetBuffer> writer, Action? callback)
        {
            if (!IsConnected) return;

            using var netBuffer = ObtainNetBuffer();
            writer.Invoke(netBuffer);

            var rentedBuffer = ArrayPool<byte>.Shared.Rent(netBuffer.Size);

            try
            {
                netBuffer.MoveTo(0).ReadBytes(rentedBuffer, 0, netBuffer.Size);

                if (!_sendChannel.Writer.TryWrite(new PendingMessage(rentedBuffer, netBuffer.Size, callback)))
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
            Send(netBuffer, tcs.SetResult);
            return tcs.Task;
        }

        public Task SendAsync(Action<INetBuffer> writer)
        {
            var tcs = new TaskCompletionSource();
            Send(writer, tcs.SetResult);
            return tcs.Task;
        }

        private INetBuffer ObtainNetBuffer()
        {
            return _netBufferPool.Obtain();
        }

        public void Disconnect()
        {
            _tcpClient.Close();
        }

        public ValueTask DisposeAsync()
        {
            _tcpClient.Dispose();
            return ValueTask.CompletedTask;
        }

        private record struct PendingMessage(byte[] RentedBuffer, int BufferSize, Action? Callback);
    }
}
