using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Threading.Channels;

namespace LiveStreamingServer.Newtorking
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
                var (netBuffer, callback) = await _sendChannel.Reader.ReadAsync(cancellationToken);

                try
                {
                    netBuffer.Flush(networkStream);
                    await networkStream.FlushAsync(cancellationToken);
                    callback?.Invoke();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "An error occurred while sending data to the client");
                }
                finally
                {
                    netBuffer.Dispose();
                }
            }
        }

        public void Send(INetBuffer netBuffer, Action? callback)
        {
            Send(netBuffer.CopyAllTo, callback);
        }

        public void Send(Action<INetBuffer> writer, Action? callback)
        {
            if (!IsConnected) return;

            var netBuffer = ObtainNetBuffer();

            try
            {
                writer.Invoke(netBuffer);
            }
            catch (Exception)
            {
                netBuffer.Dispose();
                throw;
            }

            if (!_sendChannel.Writer.TryWrite(new PendingMessage(netBuffer, callback)))
            {
                throw new Exception("Failed to write to the send channel");
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

        private record struct PendingMessage(INetBuffer NetBuffer, Action? Callback);
    }
}
