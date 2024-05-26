using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class ClientManager : IClientManager
    {
        private readonly ConcurrentDictionary<uint, IClient> _clients = new();
        private readonly ConcurrentDictionary<uint, ClientTask> _clientTasks = new();
        private readonly IClientFactory _clientFactory;
        private readonly IClientHandlerFactory _clientHandlerFactory;
        private readonly IServerEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;

        private uint _nextClientId;

        public ClientManager(
            IClientFactory clientFactory,
            IClientHandlerFactory clientHandlerFactory,
            IServerEventDispatcher eventDispatcher,
            ILogger<ClientManager> logger)
        {
            _clientFactory = clientFactory;
            _clientHandlerFactory = clientHandlerFactory;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public async Task AcceptClientAsync(ITcpListenerInternal tcpListener, ServerEndPoint serverEndPoint, CancellationToken cancellationToken)
        {
            var tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken);

            await OnClientAcceptedAsync(tcpClient);

            var clientId = GetNextClientId();
            var client = CreateClient(clientId, tcpClient);

            var clientHandler = CreateClientHandler();

            _clients.TryAdd(clientId, client);
            var clientTask = client.RunAsync(clientHandler, serverEndPoint, cancellationToken);

            _clientTasks.TryAdd(clientId, new(client, clientTask));
            await OnClientConnectedAsync(client);

            _ = clientTask.ContinueWith(async _ =>
            {
                await OnClientDisconnected(client);
                _clients.TryRemove(clientId, out var _);
                _clientTasks.TryRemove(clientId, out var _);
                await client.DisposeAsync();
            });
        }

        private uint GetNextClientId()
        {
            return Interlocked.Increment(ref _nextClientId);
        }

        private IClient CreateClient(uint clientId, ITcpClientInternal tcpClient)
        {
            return _clientFactory.Create(clientId, tcpClient);
        }

        public IClientHandle? GetClient(uint clientId)
        {
            return _clients.GetValueOrDefault(clientId);
        }

        public IReadOnlyList<IClientHandle> GetClientHandles()
        {
            return _clients.Select(x => x.Value).OfType<IClientHandle>().ToList();
        }

        private IClientHandler CreateClientHandler()
        {
            return _clientHandlerFactory.CreateClientHandler();
        }

        public async Task WaitUntilAllClientTasksCompleteAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(_clientTasks.Select(x => x.Value.Task)).WithCancellation(cancellationToken);
        }

        private async Task OnClientAcceptedAsync(ITcpClient tcpClient)
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

        private record ClientTask(IClient Client, Task Task);
    }
}
