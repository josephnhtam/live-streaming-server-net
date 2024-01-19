using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Logging;
using LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Transmuxer.Internal.Services
{
    internal class TransmuxerManager : ITransmuxerManager, IAsyncDisposable
    {
        private readonly IServer _server;
        private readonly ITransmuxerFactory _transmuxerFactory;
        private readonly IInputPathResolver _inputPathResolver;
        private readonly IOutputDirectoryPathResolver _outputDirPathResolver;
        private readonly ITransmuxerEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, TransmuxerTask> _transmuxerTasks;

        public TransmuxerManager(
            IServer server,
            ITransmuxerFactory transmuxerFactory,
            IInputPathResolver inputPathResolver,
            IOutputDirectoryPathResolver outputDirPathResolver,
            ITransmuxerEventDispatcher eventDispatcher,
            ILogger<TransmuxerManager> logger)
        {
            _server = server;
            _transmuxerFactory = transmuxerFactory;
            _inputPathResolver = inputPathResolver;
            _outputDirPathResolver = outputDirPathResolver;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
            _transmuxerTasks = new ConcurrentDictionary<string, TransmuxerTask>();
        }

        public async Task StartRemuxingStreamAsync(uint clientId, string streamPath, IDictionary<string, string> _streamArguments)
        {
            var streamArguments = new Dictionary<string, string>(_streamArguments).AsReadOnly();

            var transmuxer = await _transmuxerFactory.CreateAsync(streamPath, streamArguments);

            var cts = new CancellationTokenSource();
            var task = Task.Run(() => RunTransmuxer(transmuxer, clientId, streamPath, streamArguments, cts));

            _transmuxerTasks[streamPath] = new TransmuxerTask(task, cts);
            _ = task.ContinueWith(_ => _transmuxerTasks.TryRemove(streamPath, out var task));
        }

        private async Task RunTransmuxer(
            ITransmuxer transmuxer,
            uint clientId,
            string streamPath,
            IDictionary<string, string> streamArguments,
            CancellationTokenSource cts)
        {
            var inputPath = await _inputPathResolver.ResolveInputPathAsync(streamPath, streamArguments);
            var outputDirPath = await _outputDirPathResolver.ResolveOutputDirectoryPathAsync(streamPath, streamArguments);

            try
            {
                await transmuxer.RunAsync(inputPath, outputDirPath, TransmuxerStarted, TransmuxerStopped, cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.TransmuxerError(inputPath, outputDirPath, streamPath, ex);
                _server.GetClient(clientId)?.Disconnect();
            }

            async Task TransmuxerStarted(string outputPath)
            {
                _logger.TransmuxerStarted(inputPath, outputPath, streamPath);
                await _eventDispatcher.TransmuxerStartedAsync(inputPath, outputPath, streamPath, streamArguments);
            }

            async Task TransmuxerStopped(string outputPath)
            {
                _logger.TransmuxerStopped(inputPath, outputPath, streamPath);
                await _eventDispatcher.TransmuxerStoppedAsync(inputPath, outputPath, streamPath, streamArguments);
            }
        }

        public Task StopRemuxingStreamAsync(uint clientId, string streamPath)
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
