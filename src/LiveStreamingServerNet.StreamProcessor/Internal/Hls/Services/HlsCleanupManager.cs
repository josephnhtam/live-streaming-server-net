using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
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

        public async ValueTask ScheduleCleanupAsync(string manifestPath, IList<TsSegment> tsSegments, TimeSpan cleanupDelay)
        {
            var syncLock = _syncLocks.Get(manifestPath);

            await syncLock.WaitAsync();

            try
            {
                await ExecuteCleanupAsyncCore(manifestPath);

                var delayCts = new CancellationTokenSource();
                var delay = CalculateCleanupDelay(tsSegments, cleanupDelay);
                var cleanupTask = CleanupAsync(manifestPath, tsSegments, delay, delayCts.Token);

                _pendingCleanups[manifestPath] = new PendingCleanup(manifestPath, tsSegments, cleanupTask, delayCts);
                _ = cleanupTask.ContinueWith(_ => _pendingCleanups.TryRemove(manifestPath, out var _), TaskContinuationOptions.ExecuteSynchronously);
            }
            finally
            {
                syncLock.Release();
            }
        }

        private static TimeSpan CalculateCleanupDelay(IList<TsSegment> tsSegments, TimeSpan cleanupDelay)
        {
            if (!tsSegments.Any())
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(tsSegments.Count * tsSegments.Max(x => x.Duration)) + cleanupDelay;
        }

        private async Task CleanupAsync(string manifestPath, IList<TsSegment> tsSegments, TimeSpan delay, CancellationToken delayCancellation)
        {
            try
            {
                await Task.Delay(delay, delayCancellation);
            }
            catch (OperationCanceledException) { }

            try
            {
                File.Delete(manifestPath);

                foreach (var tsSegment in tsSegments)
                    File.Delete(tsSegment.FilePath);

                _logger.HlsCleanedUp(manifestPath);
            }
            catch (Exception ex)
            {
                _logger.CleaningUpHlsError(manifestPath, ex);
            }
        }

        private record PendingCleanup(string ManifestPath, IList<TsSegment> TsSegments, Task task, CancellationTokenSource delayCts);
    }
}
