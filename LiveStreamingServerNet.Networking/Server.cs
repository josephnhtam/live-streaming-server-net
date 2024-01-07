using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Newtorking.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Newtorking
{
    internal sealed class Server : IServer
    {
        private readonly ConcurrentDictionary<uint, ClientTask> _clientTasks = new();
        private readonly IServiceProvider _services;
        private readonly IClientHandlerFactory _clientHandlerFactory;
        private readonly IServerEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;
        private int _isStarted;
        private uint _nextClientId;

        public bool IsStarted => _isStarted == 1;
        public IList<IClientHandle> Clients => _clientTasks.Select(x => x.Value.Client).OfType<IClientHandle>().ToList();

        IReadOnlyList<IClientHandle> IServerHandle.Clients => Clients.AsReadOnly();

        public Server(
            IServiceProvider services,
            IClientHandlerFactory clientHandlerFactory,
            IServerEventDispatcher eventDispatcher,
            ILogger<Server> logger)
        {
            _services = services;
            _clientHandlerFactory = clientHandlerFactory;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public async Task RunAsync(IPEndPoint localEndpoint, CancellationToken cancellationToken = default)
        {
            ValidateAndSetStarted();

            using var tcpListener = await CreateTcpListener(localEndpoint);
            tcpListener.Start();

            await OnServerStartedAsync();

            _logger.ServerStarted(localEndpoint.ToString());

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await AcceptClientAsync(tcpListener, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.ServerShuttingDown();
            }
            catch (SocketException ex)
            {
                _logger.AcceptClientError(ex);
            }
            catch (Exception ex)
            {
                _logger.ServerLoopError(ex);
            }

            await Task.WhenAll(_clientTasks.Select(x => x.Value.Task));

            await OnServerStoppedAsync();

            _logger.ServerStopped();
        }

        private async Task AcceptClientAsync(TcpListener tcpListener, CancellationToken cancellationToken)
        {
            var tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken);
            await OnClientAcceptedAsync(tcpClient);

            var clientId = GetNextClientId();
            var client = CreateClient(clientId, tcpClient);

            var clientHandler = CreateClientHandler(client);
            var clientTask = client.RunAsync(clientHandler, cancellationToken);

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

        private uint GetNextClientId()
        {
            return Interlocked.Increment(ref _nextClientId);
        }

        private async Task<TcpListener> CreateTcpListener(IPEndPoint localEndpoint)
        {
            var listener = new TcpListener(localEndpoint);
            await OnListenerCreatedAsync(listener);
            return listener;
        }

        private IClient CreateClient(uint clientId, TcpClient tcpClient)
        {
            var client = _services.GetRequiredService<IClient>();
            client.Initialize(clientId, tcpClient);

            return client;
        }

        public IClientHandle? GetClient(uint clientId)
        {
            return _clientTasks.GetValueOrDefault(clientId)?.Client;
        }

        private IClientHandler CreateClientHandler(IClientHandle client)
        {
            return _clientHandlerFactory.CreateClientHandler(client);
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
        }

        private async Task OnServerStoppedAsync()
        {
            await _eventDispatcher.ServerStoppedAsync();
        }

        private record ClientTask(IClient Client, Task Task);
    }
}
