using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Parsers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Parsers.Contracts;
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
            var lastSegments = new List<Segment>();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    lastPollingTime = await DelayAsync(lastPollingTime, cancellationToken);

                    var playlist = ParsePlaylist();
                    if (playlist == null)
                        continue;

                    lastSegments = await PerformDeltaUploadAsync(playlist, lastSegments, cancellationToken);
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
                await Task.WhenAll(_storageAdapters.Select(async adapter =>
                    {
                        if (!_uploadedOnce.ContainsKey(adapter))
                            return;

                        await _eventDispatcher.HlsFilesStoringCompleteAsync(_context);
                    }
                ));
            }
        }

        private IPlaylist? ParsePlaylist()
        {
            try
            {
                if (!File.Exists(_context.OutputPath))
                    return null;

                return ManifestParser.Parse(_context.OutputPath);
            }
            catch (IOException)
            {
                return null;
            }
        }

        private async Task<List<Segment>> PerformDeltaUploadAsync(
            IPlaylist playlist,
            List<Segment> lastSegments,
            CancellationToken cancellationToken)
        {
            var manifests = playlist.Manifests;
            var segments = playlist.Segments;

            var addedSegments = segments.Except(lastSegments).ToList();
            var updatedManifests = addedSegments.Select(x => x.ManifestName).Distinct().Select(x => manifests[x]).ToList();

            await Task.WhenAll(_storageAdapters.Select(adapter => StoreAsync(adapter, updatedManifests, addedSegments)));

            if (_config.DeleteOutdatedSegments)
            {
                var removedSegments = lastSegments.Except(segments).ToList();
                await Task.WhenAll(_storageAdapters.Select(adapter => DeleteOutdatedAsync(adapter, removedSegments)));
            }

            return new List<Segment>(segments);

            async Task StoreAsync(IHlsStorageAdapter adapter, IReadOnlyList<Manifest> updatedManifests,
                IReadOnlyList<Segment> addedSegments)
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

                    var (storedManifests, storedSegments) =
                        await adapter.StoreAsync(_context, manifestsToUpload, addedSegments, cancellationToken);

                    _uploadedOnce[adapter] = true;

                    await _eventDispatcher.HlsFilesStoredAsync(_context, isInitial, storedManifests, storedSegments);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.UploadingHlsToStoreError(
                        _context.Processor, _context.Identifier, _context.InputPath, _context.OutputPath, _context.StreamPath, ex);
                }
            }

            async Task DeleteOutdatedAsync(IHlsStorageAdapter adapter, IReadOnlyList<Segment> removedSegments)
            {
                try
                {
                    await adapter.DeleteAsync(_context, removedSegments, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.DeletingOutdatedSegmentsError(
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
