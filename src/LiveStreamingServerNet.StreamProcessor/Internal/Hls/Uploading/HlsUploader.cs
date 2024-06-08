using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Configurations;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8Parsing;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8Parsing.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading
{
    internal class HlsUploader : IHlsUploader
    {
        private readonly IServer _server;
        private readonly StreamProcessingContext _context;
        private readonly IHlsStorageEventDispatcher _eventDispatcher;
        private readonly IEnumerable<IHlsStorageAdapter> _storageAdapters;
        private readonly ILogger _logger;
        private readonly HlsUploaderConfiguration _config;

        private readonly ConcurrentDictionary<IHlsStorageAdapter, bool> _uploadedOnce;

        public HlsUploader(
            StreamProcessingContext context,
            IServer server,
            IHlsStorageEventDispatcher eventDispatcher,
            IEnumerable<IHlsStorageAdapter> storageAdapters,
            ILogger<HlsUploader> logger,
            IOptions<HlsUploaderConfiguration> config)
        {
            _context = context;
            _server = server;
            _eventDispatcher = eventDispatcher;
            _storageAdapters = storageAdapters;
            _logger = logger;
            _config = config.Value;
            _uploadedOnce = new ConcurrentDictionary<IHlsStorageAdapter, bool>();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _uploadedOnce.Clear();

            var lastPollingTime = DateTime.UtcNow;
            var lastTsSegments = new List<ManifestTsSegment>();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    lastPollingTime = await DelayAsync(lastPollingTime, cancellationToken);

                    if (!File.Exists(_context.OutputPath))
                        continue;

                    var playlist = ManifestParser.Parse(_context.OutputPath);
                    lastTsSegments = await PerformDeltaUploadAsync(playlist, lastTsSegments, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.RunningHlsUploaderError(
                    _context.Processor, _context.Identifier, _context.InputPath, _context.OutputPath, _context.StreamPath, ex);

                _server.GetClient(_context.ClientId)?.Disconnect();
            }
            finally
            {
                await Task.WhenAll(_storageAdapters.Select(
                    async adapter =>
                    {
                        if (!_uploadedOnce.ContainsKey(adapter))
                            return;

                        await _eventDispatcher.HlsFilesStoringCompleteAsync(_context);
                    }
                ));
            }
        }

        private async Task<List<ManifestTsSegment>> PerformDeltaUploadAsync(
            IPlaylist playlist,
            List<ManifestTsSegment> lastTsSegments,
            CancellationToken cancellationToken)
        {
            var manifests = playlist.Manifests;
            var tsSegments = playlist.TsSegments;

            var addedTsSegments = tsSegments.Except(lastTsSegments).ToList();
            var updatedManifests = addedTsSegments.Select(x => x.ManifestName).Distinct().Select(x => manifests[x]).ToList();

            await Task.WhenAll(_storageAdapters.Select(adapter => StoreAsync(adapter, updatedManifests, addedTsSegments)));

            if (_config.DeleteOutdatedTsSegments)
            {
                var removedTsSegments = lastTsSegments.Except(tsSegments).ToList();
                await Task.WhenAll(_storageAdapters.Select(adapter => DeleteOutdatedAsync(adapter, removedTsSegments)));
            }

            return new List<ManifestTsSegment>(tsSegments);

            async Task StoreAsync(IHlsStorageAdapter adapter, IReadOnlyList<Manifest> updatedManifests,
                IReadOnlyList<ManifestTsSegment> addedTsSegments)
            {
                try
                {
                    bool isInitial = !_uploadedOnce.ContainsKey(adapter);

                    var manifestsToUpload = updatedManifests;

                    if (isInitial && playlist.IsMaster)
                    {
                        var includeMasterManifest = new List<Manifest> { playlist.Manifest };
                        includeMasterManifest.AddRange(updatedManifests);
                        manifestsToUpload = includeMasterManifest;
                    }

                    var (storedManifests, storedTsSegments) =
                        await adapter.StoreAsync(_context, manifestsToUpload, addedTsSegments, cancellationToken);

                    _uploadedOnce[adapter] = true;

                    await _eventDispatcher.HlsFilesStoredAsync(_context, isInitial, storedManifests, storedTsSegments);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.UploadingHlsToStoreError(
                        _context.Processor, _context.Identifier, _context.InputPath, _context.OutputPath, _context.StreamPath, ex);
                }
            }

            async Task DeleteOutdatedAsync(IHlsStorageAdapter adapter, IReadOnlyList<ManifestTsSegment> removedTsSegments)
            {
                try
                {
                    await adapter.DeleteAsync(_context, removedTsSegments, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.DeletingOutdatedTsSegmentsError(
                        _context.Processor, _context.Identifier, _context.InputPath, _context.OutputPath, _context.StreamPath, ex);
                }
            }
        }

        private async Task<DateTime> DelayAsync(DateTime lastPollingTime, CancellationToken cancellationToken)
        {
            var delay = lastPollingTime + _config.PollingInterval - DateTime.UtcNow;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, cancellationToken);

            lastPollingTime = DateTime.UtcNow;

            return lastPollingTime;
        }
    }
}
