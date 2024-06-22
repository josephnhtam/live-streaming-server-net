using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Internal.Logging;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal sealed class Server : IServer
    {
        private readonly ITcpListenerFactory _tcpListenerFactory;
        private readonly IClientManager _clientManager;
        private readonly IServerEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;
        private int _isStarted;

        public IServiceProvider Services { get; }
        public bool IsStarted => _isStarted == 1;
        public IReadOnlyList<IClientHandle> Clients => _clientManager.GetClientHandles();

        public IReadOnlyList<ServerEndPoint>? EndPoints { get; private set; }

        public Server(
            IServiceProvider services,
            ITcpListenerFactory tcpListenerFactory,
            IClientManager clientManager,
            IServerEventDispatcher eventDispatcher,
            ILogger<Server> logger)
        {
            Services = services;
            _tcpListenerFactory = tcpListenerFactory;
            _clientManager = clientManager;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public Task RunAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken = default)
        {
            return RunAsync(new List<ServerEndPoint>
            {
                serverEndPoint
            }, cancellationToken);
        }

        public async Task RunAsync(IReadOnlyList<ServerEndPoint> serverEndPoints, CancellationToken cancellationToken = default)
        {
            ValidateAndSetStarted();

            EndPoints = new List<ServerEndPoint>(serverEndPoints).AsReadOnly();

            Exception? serverException = null;
            var serverListeners = new List<ServerListener>();

            try
            {
                serverListeners = await StartServerListeners(new List<ServerEndPoint>(serverEndPoints));
                await OnServerStartedAsync();
                await RunServerLoopsAsync(serverListeners, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.ServerShuttingDown();
            }
            catch (Exception ex)
            {
                serverException = ex;
            }

            await _clientManager.WaitUntilAllClientTasksCompleteAsync();

            StopAllTcpListeners(serverListeners);

            await OnServerStoppedAsync();

            if (serverException != null)
                throw serverException;
        }

        private void StopAllTcpListeners(List<ServerListener> serverListeners)
        {
            foreach (var serverListener in serverListeners)
                serverListener.TcpListener.Stop();
        }

        private async Task RunServerLoopsAsync(List<ServerListener> serverListeners, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await Task.WhenAll(serverListeners
                .Select(x => RunServerLoopAsync(x.TcpListener, x.ServerEndPoint, cts)));
        }

        private async Task<List<ServerListener>> StartServerListeners(IReadOnlyList<ServerEndPoint> serverEndPoints)
        {
            var serverListeners = new List<ServerListener>();

            foreach (var serverEndPoint in serverEndPoints)
            {
                var tcpListener = await CreateTcpListenerAsync(serverEndPoint.LocalEndPoint);
                serverListeners.Add(new(tcpListener, serverEndPoint));
                tcpListener.Start();

                _logger.ServerStarted(serverEndPoint.LocalEndPoint.ToString());
            }

            return serverListeners;
        }

        private async Task RunServerLoopAsync(ITcpListenerInternal tcpListener, ServerEndPoint serverEndPoint, CancellationTokenSource cts)
        {
            var cancellationToken = cts.Token;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _clientManager.AcceptClientAsync(tcpListener, serverEndPoint, cancellationToken);
                }
                catch (SocketException ex)
                {
                    _logger.AcceptClientError(ex);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.ServerLoopError(ex);
                    cts.Cancel();
                    throw;
                }
            }
        }

        private void ValidateAndSetStarted()
        {
            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 1)
            {
                throw new InvalidOperationException("The server can only be started once.");
            }
        }

        private async Task<ITcpListenerInternal> CreateTcpListenerAsync(IPEndPoint localEndPoint)
        {
            var listener = _tcpListenerFactory.Create(localEndPoint);
            await OnListenerCreatedAsync(listener);
            return listener;
        }

        private async Task OnListenerCreatedAsync(ITcpListener tcpListener)
        {
            await _eventDispatcher.ListenerCreatedAsync(tcpListener);
        }

        private async Task OnServerStartedAsync()
        {
            await _eventDispatcher.ServerStartedAsync();
            _logger.ServerStarted();
        }

        private async Task OnServerStoppedAsync()
        {
            await _eventDispatcher.ServerStoppedAsync();
            _logger.ServerStopped();
        }

        public IClientHandle? GetClient(uint clientId)
        {
            return _clientManager.GetClient(clientId);
        }

        private record ServerListener(ITcpListenerInternal TcpListener, ServerEndPoint ServerEndPoint);
    }
}
