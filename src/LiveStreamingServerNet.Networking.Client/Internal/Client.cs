using LiveStreamingServerNet.Networking.Client.Contracts;
using LiveStreamingServerNet.Networking.Client.Internal.Contracts;
using LiveStreamingServerNet.Networking.Client.Internal.Logging;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Client.Internal
{
    internal class Client : IClient
    {
        private readonly ITcpClientFactory _tcpClientFactory;
        private readonly ISessionFactory _sessionFactory;
        private readonly IClientEventDispatcher _eventDispatcher;
        private readonly ILogger<Client> _logger;
        private int _started = 0;

        public IServiceProvider Services { get; }

        public Client(
            IServiceProvider services,
            ITcpClientFactory tcpClientFactory,
            ISessionFactory sessionFactory,
            IClientEventDispatcher eventDispatcher,
            ILogger<Client> logger)
        {
            Services = services;

            _tcpClientFactory = tcpClientFactory;
            _sessionFactory = sessionFactory;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public async Task RunAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _started, 1, 0) == 1)
                throw new InvalidOperationException("The client has already been started.");

            await DoRunAsync(serverEndPoint, cancellationToken);
        }

        private async Task DoRunAsync(ServerEndPoint serverEndPoint, CancellationToken stoppingToken)
        {
            Exception? clientException = null;

            try
            {
                var tcpClient = await ConnectAsync(serverEndPoint, stoppingToken);

                var session = _sessionFactory.Create(0, tcpClient, serverEndPoint);
                await OnClientConnectedAsync(session);

                await session.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.ClientStopping();
            }
            catch (Exception ex)
            {
                _logger.ClientError(ex);
                clientException = ex;
            }

            await OnClientStoppedAsync();

            if (clientException != null)
                throw clientException;
        }

        private async Task<ITcpClientInternal> ConnectAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken)
        {
            var tcpClient = _tcpClientFactory.Create();

            try
            {
                await tcpClient.Client.ConnectAsync(serverEndPoint.IPEndPoint, cancellationToken);
            }
            catch
            {
                tcpClient.Close();
                tcpClient.Dispose();
                throw;
            }

            return tcpClient;
        }

        private async Task OnClientConnectedAsync(ISessionHandle session)
        {
            await _eventDispatcher.ClientConnectedAsync(session);
            _logger.ClientConnected();
        }

        private async Task OnClientStoppedAsync()
        {
            await _eventDispatcher.ClientStoppedAsync();
            _logger.ClientStopped();
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
