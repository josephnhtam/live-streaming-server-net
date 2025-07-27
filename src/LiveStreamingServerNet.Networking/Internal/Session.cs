using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal sealed class Session : ISession
    {
        private readonly ITcpClientInternal _tcpClient;
        private readonly ServerEndPoint _serverEndPoint;
        private readonly INetworkStreamFactory _networkStreamFactory;
        private readonly ISessionHandlerFactory _sessionHandlerFactory;
        private readonly ILogger _logger;

        private readonly IBufferSender _bufferSender;

        private readonly TaskCompletionSource _stoppedTcs = new();
        private readonly CancellationTokenSource _cts = new();

        public uint Id { get; }
        public DateTime StartTime { get; }
        public bool IsConnected => _tcpClient.Connected;
        public EndPoint LocalEndPoint => _tcpClient.Client.LocalEndPoint!;
        public EndPoint RemoteEndPoint => _tcpClient.Client.RemoteEndPoint!;

        public Session(
            uint id,
            ITcpClientInternal tcpClient,
            ServerEndPoint serverEndPoint,
            IBufferSenderFactory bufferSenderFactory,
            INetworkStreamFactory networkStreamFactory,
            ISessionHandlerFactory sessionHandlerFactory,
            ILogger<Session> logger)
        {
            Id = id;
            StartTime = DateTime.UtcNow;

            _tcpClient = tcpClient;
            _serverEndPoint = serverEndPoint;
            _networkStreamFactory = networkStreamFactory;
            _sessionHandlerFactory = sessionHandlerFactory;
            _logger = logger;

            _bufferSender = bufferSenderFactory.Create();
        }

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, stoppingToken);
            var cancellationToken = linkedCts.Token;

            ISessionHandler? handler = null;
            INetworkStream? networkStream = null;

            try
            {
                networkStream = await CreateNetworkStreamAsync(cancellationToken).ConfigureAwait(false);
                _bufferSender.Start(networkStream, cancellationToken);

                handler = CreateSessionHandler();

                if (!await handler.InitializeAsync(cancellationToken).ConfigureAwait(false))
                {
                    return;
                }

                while (_tcpClient.Connected && !cancellationToken.IsCancellationRequested)
                {
                    if (!await handler.HandleSessionLoopAsync(networkStream, cancellationToken).ConfigureAwait(false))
                        return;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception ex) when (ex is IOException or EndOfStreamException) { }
            catch (Exception ex)
            {
                _logger.SessionLoopError(Id, ex);
            }
            finally
            {
                linkedCts.Cancel();

                await DisposeAsync(handler).ConfigureAwait(false);
                await DisposeAsync(_bufferSender).ConfigureAwait(false);
                await DisposeAsync(networkStream).ConfigureAwait(false);
                CloseTcpClient();

                _stoppedTcs.TrySetResult();
            }
        }

        private ISessionHandler CreateSessionHandler()
        {
            return _sessionHandlerFactory.Create(this);
        }

        private async Task<INetworkStream> CreateNetworkStreamAsync(CancellationToken cancellationToken)
        {
            return await _networkStreamFactory.CreateNetworkStreamAsync(Id, _tcpClient, _serverEndPoint, cancellationToken).ConfigureAwait(false);
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
            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException) { }
        }

        public async Task DisconnectAsync(CancellationToken cancellation)
        {
            Disconnect();

            try
            {
                await _stoppedTcs.Task.WithCancellation(cancellation).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        }

        private async Task DisposeAsync(IAsyncDisposable? disposable)
        {
            try
            {
                if (disposable != null)
                {
                    await disposable.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.DisposeError(Id, ex);
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
                _logger.CloseTcpClientError(Id, ex);
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
