using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.StreamProcessor.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Services
{
    internal class StreamProcessorManager : IStreamProcessorManager
    {
        private readonly IServer _server;
        private readonly IEnumerable<IStreamProcessorFactory> _processorFactories;
        private readonly IInputPathResolver _inputPathResolver;
        private readonly IStreamProcessorEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, StreamProcessorTask> _processorTasks;

        public StreamProcessorManager(
            IServer server,
            IEnumerable<IStreamProcessorFactory> processorFactories,
            IInputPathResolver inputPathResolver,
            IStreamProcessorEventDispatcher eventDispatcher,
            ILogger<StreamProcessorManager> logger)
        {
            _server = server;
            _processorFactories = processorFactories;
            _inputPathResolver = inputPathResolver;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
            _processorTasks = new ConcurrentDictionary<string, StreamProcessorTask>();
        }

        public async Task StartProcessingStreamAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var client = _server.GetClient(clientId);
            if (client == null) return;

            var cts = new CancellationTokenSource();

            var streamProcessors = await CreateStreamProcessors(client, streamPath, streamArguments);

            if (!streamProcessors.Any())
                return;

            await CancelAndAwaitExistingStreamProcessorsAsync(streamPath);

            var task = RunStreamProcessors(streamProcessors, client, streamPath, streamArguments, cts);

            _processorTasks[streamPath] = new StreamProcessorTask(task, cts);
            _ = task.ContinueWith(_ => _processorTasks.TryRemove(streamPath, out var task), TaskContinuationOptions.ExecuteSynchronously);

            async ValueTask CancelAndAwaitExistingStreamProcessorsAsync(string streamPath)
            {
                if (!_processorTasks.TryGetValue(streamPath, out var existingTask))
                    return;

                existingTask.Cts.Cancel();
                await existingTask.Task;
            }
        }

        private async Task<IList<IStreamProcessor>> CreateStreamProcessors(IClientHandle client, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var streamProcessors = new List<IStreamProcessor>();

            foreach (var processorFactory in _processorFactories)
            {
                var contextIdentifier = Guid.NewGuid();
                var streamProcessor = await processorFactory.CreateAsync(client, contextIdentifier, streamPath, streamArguments);

                if (streamProcessor != null)
                    streamProcessors.Add(streamProcessor);
            }

            return streamProcessors;
        }

        private async Task RunStreamProcessors(
            IList<IStreamProcessor> streamProcessors,
            IClientHandle client,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            CancellationTokenSource cts)
        {
            var tasks = new List<Task>();

            foreach (var streamProcessor in streamProcessors)
                tasks.Add(Task.Run(() => RunStreamProcessor(streamProcessor, client, streamPath, streamArguments, cts)));

            await Task.WhenAll(tasks);
        }

        private async Task RunStreamProcessor(
            IStreamProcessor streamProcessor,
            IClientHandle client,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            CancellationTokenSource cts)
        {
            var streamProcessorName = streamProcessor.Name;
            var contextIdentifier = streamProcessor.ContextIdentifier;
            var inputPath = await _inputPathResolver.ResolveInputPathAsync(streamPath, streamArguments);

            try
            {
                await streamProcessor.RunAsync(inputPath, streamPath, streamArguments, StreamProcessorStarted, StreamProcessorStopped, cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.StreamProcessorError(inputPath, streamPath, ex);
                client.Disconnect();
            }

            async Task StreamProcessorStarted(string outputPath)
            {
                _logger.StreamProcessorStarted(streamProcessorName, contextIdentifier, inputPath, outputPath, streamPath);
                await _eventDispatcher.StreamProcssorStartedAsync(streamProcessorName, contextIdentifier, client.ClientId, inputPath, outputPath, streamPath, streamArguments);
            }

            async Task StreamProcessorStopped(string outputPath)
            {
                _logger.StreamProcessorStopped(streamProcessorName, contextIdentifier, inputPath, outputPath, streamPath);
                await _eventDispatcher.StreamProcessorStoppedAsync(streamProcessorName, contextIdentifier, client.ClientId, inputPath, outputPath, streamPath, streamArguments);
            }
        }

        public Task StopProcessingStreamAsync(uint clientId, string streamPath)
        {
            if (_processorTasks.TryGetValue(streamPath, out var task))
                task.Cts.Cancel();

            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_processorTasks.Values.Select(x => x.Task));
        }

        private record StreamProcessorTask(Task Task, CancellationTokenSource Cts);
    }
}
