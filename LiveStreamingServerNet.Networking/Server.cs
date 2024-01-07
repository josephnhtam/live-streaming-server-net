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
        private readonly ConcurrentDictionary<uint, ClientTask> _clientClientTasks = new();
        private readonly IServiceProvider _services;
        private readonly IClientHandlerFactory _clientClientHandlerFactory;
        private readonly IEnumerable<IServerEventHandler> _serverEventHandlers;
        private readonly ILogger _logger;
        private int _isStarted;
        private uint _nextClientId;

        public bool IsStarted => _isStarted == 1;
        public IList<IClientHandle> Clients => _clientClientTasks.Select(x => x.Value.Client).OfType<IClientHandle>().ToList();

        public Server(
            IServiceProvider services,
            IClientHandlerFactory clientClientHandlerFactory,
            IEnumerable<IServerEventHandler> serverEventHandlers,
            ILogger<Server> logger)
        {
            _services = services;
            _clientClientHandlerFactory = clientClientHandlerFactory;
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

            await Task.WhenAll(_clientClientTasks.Select(x => x.Value.Task));

            await OnServerStoppedAsync();

            _logger.ServerStopped();
        }

        private async Task AcceptClientAsync(TcpListener tcpListener, CancellationToken cancellationToken)
        {
            var client = await tcpListener.AcceptTcpClientAsync(cancellationToken);
            await OnClientAcceptedAsync(client);

            var clientClientId = GetNextClientId();
            var clientClient = CreateClient(clientClientId, client);

            var clientClientHandler = CreateClientHandler(clientClient);
            var clientTask = clientClient.RunAsync(clientClientHandler, cancellationToken);

            _clientClientTasks.TryAdd(clientClientId, new(clientClient, clientTask));
            await OnClientConnectedAsync(clientClient);

            _ = clientTask.ContinueWith(async _ =>
            {
                await OnClientDisconnected(clientClient);
                _clientClientTasks.TryRemove(clientClientId, out var removed);
                await clientClient.DisposeAsync();
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

        private IClient CreateClient(uint clientClientId, TcpClient tcpClient)
        {
            var clientClient = _services.GetRequiredService<IClient>();
            clientClient.Initialize(clientClientId, tcpClient);

            return clientClient;
        }

        public IClientHandle? GetClient(uint clientClientId)
        {
            return _clientClientTasks.GetValueOrDefault(clientClientId)?.Client;
        }

        private IClientHandler CreateClientHandler(IClientHandle clientClient)
        {
            return _clientClientHandlerFactory.CreateClientHandler(clientClient);
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

        private async Task OnClientConnectedAsync(IClient clientClient)
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnClientConnectedAsync(clientClient);
            }
        }

        private async Task OnClientDisconnected(IClient clientClient)
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnClientDisconnectedAsync(clientClient);
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
