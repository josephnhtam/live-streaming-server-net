﻿using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvClient : IFlvClient
    {
        public string ClientId { get; private set; } = default!;
        public string StreamPath { get; private set; } = default!;
        public CancellationToken StoppingToken { get; private set; } = default!;
        public IFlvWriter FlvWriter { get; }

        private readonly IFlvMediaTagManagerService _mediaTagManager;
        private readonly TaskCompletionSource _initializationTcs = new();

        private CancellationTokenSource? _stoppingCts;
        private TaskCompletionSource? _taskCompletionSource;
        private Task? _initializationTask;
        private Task? _completeTask;

        public FlvClient(IFlvMediaTagManagerService mediaTagManager, IFlvWriter flvWriter)
        {
            _mediaTagManager = mediaTagManager;
            FlvWriter = flvWriter;
        }

        public void Initialize(string clientId, string streamPath, IStreamWriter streamWriter, CancellationToken stoppingToken)
        {
            ClientId = clientId;
            StreamPath = streamPath;
            FlvWriter.Initialize(this, streamWriter);

            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            StoppingToken = _stoppingCts.Token;

            _taskCompletionSource = new TaskCompletionSource();
            _stoppingCts.Token.Register(() => _taskCompletionSource.TrySetResult());

            _initializationTask = _initializationTcs.Task;
            _completeTask = _taskCompletionSource.Task;

            _mediaTagManager.RegisterClient(this);
        }

        public void CompleteInitialization()
        {
            _initializationTcs.SetResult();
        }

        public Task UntilIntializationComplete()
        {
            return _initializationTask ?? Task.CompletedTask;
        }

        public Task UntilComplete()
        {
            return _completeTask ?? Task.CompletedTask;
        }

        public void Stop()
        {
            _stoppingCts?.Cancel();
        }

        public async ValueTask DisposeAsync()
        {
            _mediaTagManager.UnregisterClient(this);

            if (_stoppingCts != null)
                _stoppingCts.Dispose();

            await FlvWriter.DisposeAsync();
        }
    }
}
