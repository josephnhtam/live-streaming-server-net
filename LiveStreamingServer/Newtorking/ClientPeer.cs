using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking.Contracts;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Threading.Channels;

namespace LiveStreamingServer.Newtorking
{
    public class ClientPeer : IClientPeer
    {
        private readonly TcpClient _tcpClient;
        private readonly INetBufferPool _netBufferPool;
        private readonly ILogger? _logger;
        private readonly Channel<INetBuffer> _sendChannel;

        public uint PeerId { get; }

        public ClientPeer(uint peerId, TcpClient tcpClient, INetBufferPool netBufferPool, ILogger? logger)
        {
            PeerId = peerId;
            _tcpClient = tcpClient;
            _netBufferPool = netBufferPool;
            _logger = logger;
            _sendChannel = Channel.CreateUnbounded<INetBuffer>();
        }

        public bool IsConnected => _tcpClient.Connected;

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
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred");
            }
            finally
            {
                _tcpClient.Close();
            }
        }

        private async Task SendOutstandingBuffersAsync(NetworkStream networkStream, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var netBuffer = await _sendChannel.Reader.ReadAsync(cancellationToken);

                try
                {
                    netBuffer.Flush(networkStream);
                    await networkStream.FlushAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "An error occurred while sending data to the client");
                }
                finally
                {
                    RecycleNetBuffer(netBuffer);
                }
            }
        }

        public void Send(INetBuffer netBuffer)
        {
            Send(netBuffer.Flush);
        }

        public void Send(Action<INetBuffer> callback)
        {
            if (!IsConnected) return;

            var netBuffer = ObtainNetBuffer();

            try
            {
                callback.Invoke(netBuffer);
            }
            catch (Exception)
            {
                RecycleNetBuffer(netBuffer);
                throw;
            }

            if (!_sendChannel.Writer.TryWrite(netBuffer))
            {
                throw new Exception("Failed to write to the send channel");
            }
        }

        public async Task SendAsync(Func<INetBuffer, Task> callback)
        {
            if (!IsConnected) return;

            var netBuffer = ObtainNetBuffer();

            try
            {
                await callback.Invoke(netBuffer);
            }
            catch (Exception)
            {
                RecycleNetBuffer(netBuffer);
                throw;
            }

            await _sendChannel.Writer.WriteAsync(netBuffer);
        }

        private INetBuffer ObtainNetBuffer()
        {
            return _netBufferPool.ObtainNetBuffer();
        }

        private void RecycleNetBuffer(INetBuffer netBuffer)
        {
            _netBufferPool.RecycleNetBuffer(netBuffer);
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
    }
}
