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
    public sealed class Server : IServer
    {
        private readonly ConcurrentDictionary<uint, ClientTask> _clientTasks = new();
        private readonly IServiceProvider _services;
        private readonly IClientHandlerFactory _clientHandlerFactory;
        private readonly IEnumerable<IServerEventHandler> _serverEventHandlers;
        private readonly ILogger _logger;
        private int _isStarted;
        private uint _nextClientId;

        public bool IsStarted => _isStarted == 1;
        public IList<IClientHandle> Clients => _clientTasks.Select(x => x.Value.Client).OfType<IClientHandle>().ToList();

        public Server(
            IServiceProvider services,
            IClientHandlerFactory clientHandlerFactory,
            IEnumerable<IServerEventHandler> serverEventHandlers,
            ILogger<Server> logger)
        {
            _services = services;
            _clientHandlerFactory = clientHandlerFactory;
            _serverEventHandlers = serverEventHandlers;
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
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnListenerCreatedAsync(tcpListener);
            }
        }

        private async Task OnClientAcceptedAsync(TcpClient tcpClient)
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnClientAcceptedAsync(tcpClient);
            }
        }

        private async Task OnClientConnectedAsync(IClientHandle client)
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnClientConnectedAsync(client);
            }
        }

        private async Task OnClientDisconnected(IClientHandle client)
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnClientDisconnectedAsync(client);
            }
        }

        private async Task OnServerStartedAsync()
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnServerStartedAsync();
            }
        }

        private async Task OnServerStoppedAsync()
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnServerStoppedAsync();
            }
        }

        private record ClientTask(IClient Client, Task Task);
    }
}
