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
        private readonly IOutputPathResolver _outputPathResolver;
        private readonly ITransmuxerEventDispatcher _eventDispatcher;
        private readonly ConcurrentDictionary<string, TransmuxerTask> _transmuxerTasks;

        public TransmuxerManager(
            ITransmuxerFactory transmuxerFactory,
            IInputPathResolver inputPathResolver,
            IOutputPathResolver outputPathResolver,
            ITransmuxerEventDispatcher eventDispatcher)
        {
            _transmuxerFactory = transmuxerFactory;
            _inputPathResolver = inputPathResolver;
            _outputPathResolver = outputPathResolver;
            _eventDispatcher = eventDispatcher;
            _transmuxerTasks = new ConcurrentDictionary<string, TransmuxerTask>();
        }

        public async Task StartRemuxingStreamAsync(string streamPath, IDictionary<string, string> _streamArguments)
        {
            var streamArguments = new Dictionary<string, string>(_streamArguments).AsReadOnly();

            var transmuxer = await _transmuxerFactory.CreateAsync(streamPath, streamArguments);
            var inputPath = await _inputPathResolver.ResolveInputPathAsync(streamPath, streamArguments);
            var outputPath = await _outputPathResolver.ResolveOutputPathAsync(streamPath, streamArguments);

            var cts = new CancellationTokenSource();
            var task = Task.Run(() => transmuxer.RunAsync(inputPath, outputPath, cts.Token));

            _transmuxerTasks[streamPath] = new TransmuxerTask(streamPath, streamArguments, inputPath, outputPath, task, cts);
            await _eventDispatcher.TransmuxerStartedAsync(inputPath, outputPath, streamPath, streamArguments);

            _ = task.ContinueWith(async _ =>
            {
                if (!_transmuxerTasks.TryRemove(streamPath, out var task))
                    return;

                await _eventDispatcher.TransmuxerStoppedAsync(task.InputPath, task.OutputPath, task.StreamPath, task.StreamArguments);
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
            string OutputPath,
            Task Task,
            CancellationTokenSource Cts);
    }
}
