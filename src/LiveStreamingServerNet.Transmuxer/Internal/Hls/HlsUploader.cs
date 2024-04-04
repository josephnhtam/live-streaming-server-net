using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Hls;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls
{
    internal class HlsUploader : IHlsUploader
    {
        private readonly TransmuxingContext _context;
        private readonly IHlsStorageEventDispatcher _eventDispatcher;
        private readonly IEnumerable<IHlsStorageAdapter> _storageAdapters;
        private readonly ILogger _logger;
        private readonly HlsUploaderConfiguration _config;

        private readonly ConcurrentDictionary<IHlsStorageAdapter, bool> _uploadedOnce;

        public HlsUploader(
            TransmuxingContext context,
            IHlsStorageEventDispatcher eventDispatcher,
            IEnumerable<IHlsStorageAdapter> storageAdapters,
            ILogger<HlsUploader> logger,
            IOptions<HlsUploaderConfiguration> config)
        {
            _context = context;
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
            var lastTsFiles = new List<TsFile>();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    lastPollingTime = await DelayAsync(lastPollingTime, cancellationToken);

                    if (!File.Exists(_context.OutputPath))
                        continue;

                    var playlist = ManifestParser.Parse(_context.OutputPath);
                    lastTsFiles = await PerformDeltaUploadAsync(playlist, lastTsFiles, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.RunningHlsUploaderError(
                    _context.Transmuxer, _context.Identifier, _context.InputPath, _context.OutputPath, _context.StreamPath, ex);
            }
            finally
            {
                await Task.WhenAll(_storageAdapters.Select(
                    async adapter =>
                    {
                        var uploadedOnce = _uploadedOnce.ContainsKey(adapter);
                        await _eventDispatcher.HlsFilesStoringCompleteAsync(_context);
                    }
                ));
            }
        }

        private async Task<List<TsFile>> PerformDeltaUploadAsync(
            IPlaylist playlist,
            List<TsFile> lastTsFiles,
            CancellationToken cancellationToken)
        {
            var manifests = playlist.Manifests;
            var tsFiles = playlist.TsFiles;

            var deltaTsFiles = tsFiles.Except(lastTsFiles).ToList();
            var deltaManifests = deltaTsFiles.Select(x => x.ManifestName).Distinct().Select(x => manifests[x]).ToList();

            await Task.WhenAll(_storageAdapters.Select(adapter => StoreAsync(adapter, deltaManifests, deltaTsFiles)));
            return new List<TsFile>(tsFiles);

            async Task StoreAsync(IHlsStorageAdapter adapter, IReadOnlyList<Manifest> deltaManifests, IReadOnlyList<TsFile> deltaTsFiles)
            {
                try
                {
                    var (storedManifests, storedTsFiles) =
                        await adapter.StoreAsync(_context, deltaManifests, deltaTsFiles, cancellationToken);

                    bool isInitial = !_uploadedOnce.ContainsKey(adapter);
                    _uploadedOnce[adapter] = true;

                    await _eventDispatcher.HlsFilesStoredAsync(_context, isInitial, storedManifests, storedTsFiles);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.UploadingHlsToStoreError(
                        _context.Transmuxer, _context.Identifier, _context.InputPath, _context.OutputPath, _context.StreamPath, ex);
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
