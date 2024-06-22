using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Common.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services
{
    internal class HlsCleanupManager : IHlsCleanupManager
    {
        private readonly ConcurrentDictionary<string, PendingCleanup> _pendingCleanups;
        private readonly ILogger _logger;

        private readonly IItemCollection<SemaphoreSlim> _syncLocks;

        public HlsCleanupManager(ILogger<HlsCleanupManager> logger)
        {
            _pendingCleanups = new ConcurrentDictionary<string, PendingCleanup>();
            _logger = logger;

            _syncLocks = new HashedItemCollection<SemaphoreSlim>(
                8 * Math.Max(1, Environment.ProcessorCount),
                _ => new SemaphoreSlim(1, 1));
        }

        public async ValueTask ExecuteCleanupAsync(string manifestPath)
        {
            var syncLock = _syncLocks.Get(manifestPath);

            await syncLock.WaitAsync();

            try
            {
                await ExecuteCleanupAsyncCore(manifestPath);
            }
            finally
            {
                syncLock.Release();
            }
        }

        private async ValueTask ExecuteCleanupAsyncCore(string manifestPath)
        {
            if (!_pendingCleanups.TryGetValue(manifestPath, out var cleanupTask))
                return;

            if (!cleanupTask.delayCts.IsCancellationRequested)
                cleanupTask.delayCts.Cancel();

            await cleanupTask.task;
        }

        public async ValueTask ScheduleCleanupAsync(string manifestPath, IList<string> files, TimeSpan cleanupDelay)
        {
            var syncLock = _syncLocks.Get(manifestPath);

            await syncLock.WaitAsync();

            try
            {
                await ExecuteCleanupAsyncCore(manifestPath);

                var delayCts = new CancellationTokenSource();
                var cleanupTask = CleanupAsync(manifestPath, files, cleanupDelay, delayCts.Token);

                _pendingCleanups[manifestPath] = new PendingCleanup(manifestPath, files, cleanupTask, delayCts);
                _ = cleanupTask.ContinueWith(_ => _pendingCleanups.TryRemove(manifestPath, out var _), TaskContinuationOptions.ExecuteSynchronously);
            }
            finally
            {
                syncLock.Release();
            }
        }

        private async Task CleanupAsync(string manifestPath, IList<string> files, TimeSpan delay, CancellationToken delayCancellation)
        {
            try
            {
                await Task.Delay(delay, delayCancellation);
            }
            catch (OperationCanceledException) { }

            try
            {
                foreach (var file in files)
                    File.Delete(file);

                _logger.HlsCleanedUp(manifestPath);
            }
            catch (Exception ex)
            {
                _logger.HlsCleanupError(manifestPath, ex);
            }
        }

        private record PendingCleanup(string ManifestPath, IList<string> Files, Task task, CancellationTokenSource delayCts);
    }
}
