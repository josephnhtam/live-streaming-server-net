using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking
{
    internal sealed class Server : IServer
    {
        private readonly ConcurrentDictionary<uint, ClientTask> _clientTasks = new();
        private readonly IClientHandlerFactory _clientHandlerFactory;
        private readonly IServerEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;
        private int _isStarted;
        private uint _nextClientId;

        public IServiceProvider Services { get; }
        public bool IsStarted => _isStarted == 1;
        public IList<IClientHandle> Clients => GetClientHandles();

        public IReadOnlyList<ServerEndPoint>? EndPoints { get; private set; }
        IReadOnlyList<IClientHandle> IServerHandle.Clients => Clients.AsReadOnly();

        public Server(
            IServiceProvider services,
            IClientHandlerFactory clientHandlerFactory,
            IServerEventDispatcher eventDispatcher,
            ILogger<Server> logger)
        {
            Services = services;
            _clientHandlerFactory = clientHandlerFactory;
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

        public async Task RunAsync(IList<ServerEndPoint> serverEndPoints, CancellationToken cancellationToken = default)
        {
            ValidateAndSetStarted();

            Exception? serverException = null;
            var serverListeners = new List<ServerListener>();

            try
            {
                serverListeners = await StartServerListeners(serverEndPoints);
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

            await Task.WhenAll(_clientTasks.Select(x => x.Value.Task));

            foreach (var serverListener in serverListeners)
                serverListener.TcpListener.Dispose();

            await OnServerStoppedAsync();

            if (serverException != null)
                throw serverException;
        }

        private async Task RunServerLoopsAsync(List<ServerListener> serverListeners, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await Task.WhenAll(serverListeners
                .Select(x => RunServerLoopAsync(x.TcpListener, x.ServerEndPoint, cts)));
        }

        private async Task<List<ServerListener>> StartServerListeners(IList<ServerEndPoint> serverEndPoints)
        {
            var serverListeners = new List<ServerListener>();

            foreach (var serverEndPoint in serverEndPoints)
            {
                var tcpListener = await CreateTcpListenerAsync(serverEndPoint.LocalEndPoint);
                serverListeners.Add(new(tcpListener, serverEndPoint));
                tcpListener.Start();

                _logger.ServerStarted(serverEndPoint.LocalEndPoint.ToString());
            }

            EndPoints = new List<ServerEndPoint>(serverEndPoints).AsReadOnly();

            return serverListeners;
        }

        private async Task RunServerLoopAsync(TcpListener tcpListener, ServerEndPoint serverEndPoint, CancellationTokenSource cts)
        {
            var cancellationToken = cts.Token;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                    await AcceptClientAsync(tcpListener, serverEndPoint, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (SocketException ex)
            {
                _logger.AcceptClientError(ex);
                cts.Cancel();
                throw;
            }
            catch (Exception ex)
            {
                _logger.ServerLoopError(ex);
                cts.Cancel();
                throw;
            }
        }

        private async Task AcceptClientAsync(TcpListener tcpListener, ServerEndPoint serverEndPoint, CancellationToken cancellationToken)
        {
            var tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken);
            tcpClient.NoDelay = true;

            await OnClientAcceptedAsync(tcpClient);

            var clientId = GetNextClientId();
            var client = CreateClient(clientId, tcpClient);

            var clientHandler = CreateClientHandler();
            var clientTask = client.RunAsync(clientHandler, serverEndPoint, cancellationToken);

            _clientTasks.TryAdd(clientId, new(client, clientTask));
            await OnClientConnectedAsync(client);

            _ = clientTask.ContinueWith(async _ =>
            {
                await OnClientDisconnected(client);
                _clientTasks.TryRemove(clientId, out var removed);
                await client.DisposeAsync();
            });
        }

        private void ValidateAndSetStarted()
        {
            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 1)
            {
                throw new InvalidOperationException("The server can only be started once.");
            }
        }

        private List<IClientHandle> GetClientHandles()
        {
            return _clientTasks.Select(x => x.Value.Client).OfType<IClientHandle>().ToList();
        }

        private uint GetNextClientId()
        {
            return Interlocked.Increment(ref _nextClientId);
        }

        private async Task<TcpListener> CreateTcpListenerAsync(IPEndPoint localEndPoint)
        {
            var listener = new TcpListener(localEndPoint);
            await OnListenerCreatedAsync(listener);
            return listener;
        }

        private IClient CreateClient(uint clientId, TcpClient tcpClient)
        {
            var client = Services.GetRequiredService<IClient>();
            client.Initialize(clientId, tcpClient);

            return client;
        }

        public IClientHandle? GetClient(uint clientId)
        {
            return _clientTasks.GetValueOrDefault(clientId)?.Client;
        }

        private IClientHandler CreateClientHandler()
        {
            return _clientHandlerFactory.CreateClientHandler();
        }

        private async Task OnListenerCreatedAsync(TcpListener tcpListener)
        {
            await _eventDispatcher.ListenerCreatedAsync(tcpListener);
        }

        private async Task OnClientAcceptedAsync(TcpClient tcpClient)
        {
            await _eventDispatcher.ClientAcceptedAsync(tcpClient);
        }

        private async Task OnClientConnectedAsync(IClientHandle client)
        {
            await _eventDispatcher.ClientConnectedAsync(client);
        }

        private async Task OnClientDisconnected(IClientHandle client)
        {
            await _eventDispatcher.ClientDisconnectedAsync(client);
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

        private record ServerListener(TcpListener TcpListener, ServerEndPoint ServerEndPoint);
        private record ClientTask(IClient Client, Task Task);
    }
}
