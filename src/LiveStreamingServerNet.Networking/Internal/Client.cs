using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal sealed class Client : IClient
    {
        private readonly IClientBufferSender _bufferSender;
        private readonly INetworkStreamFactory _networkStreamFactory;
        private readonly ILogger _logger;

        private ITcpClientInternal _tcpClient = default!;
        private CancellationTokenSource? _cts;
        private TaskCompletionSource _stoppedTcs = new();

        public uint ClientId { get; }

        public Client(
            uint clientId,
            ITcpClientInternal tcpClient,
            IClientBufferSender bufferSender,
            INetworkStreamFactory networkStreamFactory,
            ILogger<Client> logger)
        {
            ClientId = clientId;
            _tcpClient = tcpClient;
            _bufferSender = bufferSender;
            _networkStreamFactory = networkStreamFactory;
            _logger = logger;
        }

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public EndPoint LocalEndPoint => _tcpClient.Client.LocalEndPoint!;
        public EndPoint RemoteEndPoint => _tcpClient.Client.RemoteEndPoint!;

        public async Task RunAsync(IClientHandler handler, ServerEndPoint serverEndPoint, CancellationToken stoppingToken)
        {
            _logger.ClientConnected(ClientId);

            await handler.InitializeAsync(this);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var cancellationToken = _cts.Token;

            INetworkStream? networkStream = null;

            try
            {
                networkStream = await CreateNetworkStreamAsync(serverEndPoint, cancellationToken);
                _bufferSender.Start(networkStream, cancellationToken);

                while (_tcpClient.Connected && !cancellationToken.IsCancellationRequested)
                {
                    if (!await handler.HandleClientLoopAsync(networkStream, cancellationToken))
                        break;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception ex) when (ex is IOException or EndOfStreamException) { }
            catch (Exception ex)
            {
                _logger.ClientLoopError(ClientId, ex);
            }
            finally
            {
                _cts.Cancel();
            }

            await _bufferSender.DisposeAsync();
            await handler.DisposeAsync();
            networkStream?.Dispose();
            _tcpClient.Close();

            _logger.ClientDisconnected(ClientId);
        }

        public void Send(IDataBuffer dataBuffer, Action<bool>? callback)
        {
            _bufferSender.Send(dataBuffer, callback);
        }

        public void Send(IRentedBuffer rentedBuffer, Action<bool>? callback)
        {
            _bufferSender.Send(rentedBuffer, callback);
        }

        public void Send(Action<IDataBuffer> writer, Action<bool>? callback)
        {
            _bufferSender.Send(writer, callback);
        }

        public ValueTask SendAsync(IDataBuffer dataBuffer)
        {
            return _bufferSender.SendAsync(dataBuffer);
        }

        public ValueTask SendAsync(IRentedBuffer rentedBuffer)
        {
            return _bufferSender.SendAsync(rentedBuffer);
        }

        public ValueTask SendAsync(Action<IDataBuffer> writer)
        {
            return _bufferSender.SendAsync(writer);
        }

        public void Disconnect()
        {
            _cts?.Cancel();
        }

        public async Task DisconnectAsync(CancellationToken cancellation)
        {
            Disconnect();
            await _stoppedTcs.Task.WithCancellation(cancellation);
        }

        public ValueTask DisposeAsync()
        {
            _tcpClient.Close();
            _stoppedTcs.TrySetResult();
            return ValueTask.CompletedTask;
        }

        private async Task<INetworkStream> CreateNetworkStreamAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken)
        {
            return await _networkStreamFactory.CreateNetworkStreamAsync(_tcpClient, serverEndPoint, cancellationToken);
        }
    }
}
