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
        private readonly Channel<PendingMessage> _pendingMessageChannel;
        private TcpClient _tcpClient = default!;

        public uint PeerId { get; private set; }

        public ClientPeer(IServiceProvider services)
        {
            _netBufferPool = services.GetRequiredService<INetBufferPool>();
            _logger = services.GetRequiredService<ILogger<ClientPeer>>();
            _pendingMessageChannel = Channel.CreateUnbounded<PendingMessage>();
        }

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public void Initialize(uint peerId, TcpClient tcpClient)
        {
            PeerId = peerId;
            _tcpClient = tcpClient;
        }

        public async Task RunAsync(IClientPeerHandler handler, CancellationToken stoppingToken)
        {
            await using (var outstandingBufferSender = new OutstandingBufferSender(_pendingMessageChannel.Reader, _logger))
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                var cancellationToken = cts.Token;

                try
                {
                    var networkStream = _tcpClient.GetStream();
                    var readOnlyNetworkStream = new ReadOnlyNetworkStream(networkStream);
                    outstandingBufferSender.Start(networkStream, cancellationToken);

                    while (_tcpClient.Connected && !cancellationToken.IsCancellationRequested)
                        if (!await handler.HandleClientPeerLoopAsync(readOnlyNetworkStream, cancellationToken))
                            break;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception ex) when (ex is IOException or EndOfStreamException) { }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "An error occurred");
                }
                finally
                {
                    cts.Cancel();
                }
            }

            await handler.DisposeAsync();
            _tcpClient.Close();

            _logger?.LogDebug("PeerId: {PeerId} | Disconnected", PeerId);
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

        public void Send(Action<INetBuffer> writer, Action? callback)
        {
            if (!IsConnected) return;

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

        private class OutstandingBufferSender : IAsyncDisposable
        {
            private readonly ChannelReader<PendingMessage> _pendingMessageReader;
            private readonly ILogger? _logger;

            private Task? _task;

            public OutstandingBufferSender(ChannelReader<PendingMessage> pendingMessageReader, ILogger? logger)
            {
                _pendingMessageReader = pendingMessageReader;
                _logger = logger;
            }

            public void Start(NetworkStream networkStream, CancellationToken cancellationToken)
            {
                _task = Task.Run(() => SendOutstandingBuffersAsync(networkStream, cancellationToken));
            }

            private async Task SendOutstandingBuffersAsync(NetworkStream networkStream, CancellationToken cancellationToken)
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var (rentedBuffer, bufferSize, callback) = await _pendingMessageReader.ReadAsync(cancellationToken);

                        try
                        {
                            await networkStream.WriteAsync(rentedBuffer, 0, bufferSize, cancellationToken);
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                        catch (IOException) { }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "An error occurred while sending data to the client");
                        }
                        finally
                        {
                            callback?.Invoke();
                            ArrayPool<byte>.Shared.Return(rentedBuffer);
                        }
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            }

            public async ValueTask DisposeAsync()
            {
                if (_task != null)
                    await _task;
            }
        }
    }
}
