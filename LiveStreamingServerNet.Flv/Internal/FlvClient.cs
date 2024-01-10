using LiveStreamingServerNet.Flv.Internal.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvClient : IFlvClient
    {
        public IStreamWriter StreamWriter { get; private set; } = default!;

        private CancellationTokenSource? _stoppingCts;
        private TaskCompletionSource? _taskCompletionSource;
        private Task? _completeTask;

        public void Start(IStreamWriter streamWriter, CancellationToken stoppingToken)
        {
            StreamWriter = streamWriter;

            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            _taskCompletionSource = new TaskCompletionSource();
            _stoppingCts.Token.Register(() => _taskCompletionSource.TrySetResult());

            _completeTask = _taskCompletionSource.Task;
        }

        public Task UntilComplete()
        {
            return _completeTask ?? Task.CompletedTask;
        }

        public void Stop()
        {
            _stoppingCts?.Cancel();
        }

        public ValueTask DisposeAsync()
        {
            if (_stoppingCts != null)
                _stoppingCts.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
