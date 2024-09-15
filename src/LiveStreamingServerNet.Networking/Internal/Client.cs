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
        private readonly ITcpClientInternal _tcpClient;
        private readonly ServerEndPoint _serverEndPoint;
        private readonly INetworkStreamFactory _networkStreamFactory;
        private readonly IClientHandlerFactory _clientHandlerFactory;
        private readonly ILogger _logger;

        private readonly IClientBufferSender _bufferSender;
        private readonly TaskCompletionSource _stoppedTcs = new();
        private readonly CancellationTokenSource _cts = new();

        public uint ClientId { get; }
        public DateTime StartTime { get; }
        public bool IsConnected => _tcpClient.Connected;
        public EndPoint LocalEndPoint => _tcpClient.Client.LocalEndPoint!;
        public EndPoint RemoteEndPoint => _tcpClient.Client.RemoteEndPoint!;

        public Client(
            uint clientId,
            ITcpClientInternal tcpClient,
            ServerEndPoint serverEndPoint,
            IClientBufferSenderFactory senderFactory,
            INetworkStreamFactory networkStreamFactory,
            IClientHandlerFactory clientHandlerFactory,
            ILogger<Client> logger)
        {
            ClientId = clientId;
            StartTime = DateTime.UtcNow;

            _tcpClient = tcpClient;
            _serverEndPoint = serverEndPoint;
            _networkStreamFactory = networkStreamFactory;
            _clientHandlerFactory = clientHandlerFactory;
            _logger = logger;

            _bufferSender = senderFactory.Create(clientId);
        }

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            _logger.ClientConnected(ClientId);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, stoppingToken);
            var cancellationToken = linkedCts.Token;

            IClientHandler? handler = null;
            INetworkStream? networkStream = null;

            try
            {
                networkStream = await CreateNetworkStreamAsync(_serverEndPoint, cancellationToken);
                _bufferSender.Start(networkStream, cancellationToken);

                handler = CreateClientHandler();
                await handler.InitializeAsync();

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

            linkedCts.Cancel();

            await DisposeAsync(handler);
            await DisposeAsync(_bufferSender);
            await DisposeAsync(networkStream);
            CloseTcpClient();

            _stoppedTcs.TrySetResult();
            _logger.ClientDisconnected(ClientId);
        }

        private IClientHandler CreateClientHandler()
        {
            return _clientHandlerFactory.CreateClientHandler(this);
        }

        private Task<INetworkStream> CreateNetworkStreamAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken)
        {
            return _networkStreamFactory.CreateNetworkStreamAsync(_tcpClient, serverEndPoint, cancellationToken);
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
            _cts.Cancel();
        }

        public async Task DisconnectAsync(CancellationToken cancellation)
        {
            Disconnect();
            await _stoppedTcs.Task.WithCancellation(cancellation);
        }

        private async Task DisposeAsync(IAsyncDisposable? disposable)
        {
            try
            {
                if (disposable != null)
                {
                    await disposable.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.DisposeError(ClientId, ex);
            }
        }

        private void CloseTcpClient()
        {
            try
            {
                _tcpClient.Close();
            }
            catch (Exception ex)
            {
                _logger.CloseTcpClientError(ClientId, ex);
            }
        }

        public ValueTask DisposeAsync()
        {
            _cts.Dispose();
            _tcpClient.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
