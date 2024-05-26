using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Logging;
using LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Transmuxer.Internal.Services
{
    internal class TransmuxerManager : ITransmuxerManager
    {
        private readonly IServer _server;
        private readonly IEnumerable<ITransmuxerFactory> _transmuxerFactories;
        private readonly IInputPathResolver _inputPathResolver;
        private readonly ITransmuxerEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, TransmuxerTask> _transmuxerTasks;

        public TransmuxerManager(
            IServer server,
            IEnumerable<ITransmuxerFactory> transmuxerFactories,
            IInputPathResolver inputPathResolver,
            ITransmuxerEventDispatcher eventDispatcher,
            ILogger<TransmuxerManager> logger)
        {
            _server = server;
            _transmuxerFactories = transmuxerFactories;
            _inputPathResolver = inputPathResolver;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
            _transmuxerTasks = new ConcurrentDictionary<string, TransmuxerTask>();
        }

        public async Task StartTransmuxingStreamAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var client = _server.GetClient(clientId);
            if (client == null) return;

            var cts = new CancellationTokenSource();

            var transmuxers = await CreateTransmuxers(client, streamPath, streamArguments);
            var task = RunTransmuxers(transmuxers, client, streamPath, streamArguments, cts);

            _transmuxerTasks[streamPath] = new TransmuxerTask(task, cts);
            _ = task.ContinueWith(_ => _transmuxerTasks.TryRemove(streamPath, out var task));
        }

        private async Task<IList<ITransmuxer>> CreateTransmuxers(IClientHandle client, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var transmuxers = new List<ITransmuxer>();

            foreach (var transmuxerFactory in _transmuxerFactories)
            {
                var contextIdentifier = Guid.NewGuid();
                transmuxers.Add(await transmuxerFactory.CreateAsync(client, contextIdentifier, streamPath, streamArguments));
            }

            return transmuxers;
        }

        private async Task RunTransmuxers(
            IList<ITransmuxer> transmuxers,
            IClientHandle client,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            CancellationTokenSource cts)
        {
            var tasks = new List<Task>();

            foreach (var transmuxer in transmuxers)
                tasks.Add(Task.Run(() => RunTransmuxer(transmuxer, client, streamPath, streamArguments, cts)));

            await Task.WhenAll(tasks);
        }

        private async Task RunTransmuxer(
            ITransmuxer transmuxer,
            IClientHandle client,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            CancellationTokenSource cts)
        {
            var transmuxerName = transmuxer.Name;
            var contextIdentifier = transmuxer.ContextIdentifier;
            var inputPath = await _inputPathResolver.ResolveInputPathAsync(streamPath, streamArguments);

            try
            {
                await transmuxer.RunAsync(inputPath, streamPath, streamArguments, TransmuxerStarted, TransmuxerStopped, cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.TransmuxerError(inputPath, streamPath, ex);
                client.Disconnect();
            }

            async Task TransmuxerStarted(string outputPath)
            {
                _logger.TransmuxerStarted(transmuxerName, contextIdentifier, inputPath, outputPath, streamPath);
                await _eventDispatcher.TransmuxerStartedAsync(transmuxerName, contextIdentifier, client.ClientId, inputPath, outputPath, streamPath, streamArguments);
            }

            async Task TransmuxerStopped(string outputPath)
            {
                _logger.TransmuxerStopped(transmuxerName, contextIdentifier, inputPath, outputPath, streamPath);
                await _eventDispatcher.TransmuxerStoppedAsync(transmuxerName, contextIdentifier, client.ClientId, inputPath, outputPath, streamPath, streamArguments);
            }
        }

        public Task StopTransmuxingStreamAsync(uint clientId, string streamPath)
        {
            if (_transmuxerTasks.TryGetValue(streamPath, out var task))
                task.Cts.Cancel();

            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_transmuxerTasks.Values.Select(x => x.Task));
        }

        private record TransmuxerTask(Task Task, CancellationTokenSource Cts);
    }
}
