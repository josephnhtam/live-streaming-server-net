using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Transmuxer.Internal.Services
{
    internal class TransmuxerManager : ITransmuxerManager, IAsyncDisposable
    {
        private readonly ITransmuxerFactory _transmuxerFactory;
        private readonly IInputPathResolver _inputPathResolver;
        private readonly IOutputDirectoryPathResolver _outputDirPathResolver;
        private readonly ITransmuxerEventDispatcher _eventDispatcher;
        private readonly ConcurrentDictionary<string, TransmuxerTask> _transmuxerTasks;

        public TransmuxerManager(
            ITransmuxerFactory transmuxerFactory,
            IInputPathResolver inputPathResolver,
            IOutputDirectoryPathResolver outputDirPathResolver,
            ITransmuxerEventDispatcher eventDispatcher)
        {
            _transmuxerFactory = transmuxerFactory;
            _inputPathResolver = inputPathResolver;
            _outputDirPathResolver = outputDirPathResolver;
            _eventDispatcher = eventDispatcher;
            _transmuxerTasks = new ConcurrentDictionary<string, TransmuxerTask>();
        }

        public async Task StartRemuxingStreamAsync(string streamPath, IDictionary<string, string> _streamArguments)
        {
            var streamArguments = new Dictionary<string, string>(_streamArguments).AsReadOnly();

            var transmuxer = await _transmuxerFactory.CreateAsync(streamPath, streamArguments);
            var inputPath = await _inputPathResolver.ResolveInputPathAsync(streamPath, streamArguments);
            var outputDirPath = await _outputDirPathResolver.ResolveOutputDirectoryPathAsync(streamPath, streamArguments);

            var cts = new CancellationTokenSource();
            var task = Task.Run(() => transmuxer.RunAsync(inputPath, outputDirPath, cts.Token));

            _transmuxerTasks[streamPath] = new TransmuxerTask(streamPath, streamArguments, inputPath, outputDirPath, task, cts);
            await _eventDispatcher.TransmuxerStartedAsync(inputPath, outputDirPath, streamPath, streamArguments);

            _ = task.ContinueWith(async _ =>
            {
                if (!_transmuxerTasks.TryRemove(streamPath, out var task))
                    return;

                await _eventDispatcher.TransmuxerStoppedAsync(task.InputPath, task.OutputDirPath, task.StreamPath, task.StreamArguments);
            });
        }

        public Task StopRemuxingStreamAsync(string streamPath)
        {
            if (_transmuxerTasks.TryGetValue(streamPath, out var task))
                task.Cts.Cancel();

            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_transmuxerTasks.Values.Select(x => x.Task));
        }

        private record TransmuxerTask(
            string StreamPath,
            IDictionary<string, string> StreamArguments,
            string InputPath,
            string OutputDirPath,
            Task Task,
            CancellationTokenSource Cts);
    }
}
