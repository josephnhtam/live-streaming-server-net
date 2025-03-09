using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.M3u8.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using static LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.HlsTransmuxer;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal class HlsOutputHandler : IHlsOutputHandler
    {
        private readonly IManifestWriter _manifestWriter;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly Configuration _config;
        private readonly ILogger<HlsOutputHandler> _logger;
        private readonly Queue<TsSegment> _segments;

        public string Name { get; }
        public Guid ContextIdentifier { get; }
        public string StreamPath { get; }

        public HlsOutputHandler(
            IManifestWriter manifestWriter,
            IHlsCleanupManager cleanupManager,
            Configuration config,
            ILogger<HlsOutputHandler> logger)
        {
            Name = config.TransmuxerName;
            ContextIdentifier = config.ContextIdentifier;
            StreamPath = config.StreamPath;

            _manifestWriter = manifestWriter;
            _cleanupManager = cleanupManager;
            _config = config;
            _logger = logger;
            _segments = new Queue<TsSegment>();
        }

        public async ValueTask AddSegmentAsync(TsSegment segment)
        {
            await DoAddSegmentAsync(segment);
            await WriteManifestAsync();
        }

        private ValueTask DoAddSegmentAsync(TsSegment segment)
        {
            _segments.Enqueue(segment);

            if (_segments.Count > _config.SegmentListSize)
            {
                var removedSegment = _segments.Dequeue();

                if (_config.DeleteOutdatedSegments)
                    DeleteOutdatedSegments(removedSegment);
            }

            return ValueTask.CompletedTask;
        }

        private void DeleteOutdatedSegments(TsSegment removedSegment)
        {
            File.Delete(removedSegment.FilePath);
            _logger.OutdatedTsSegmentDeleted(Name, ContextIdentifier, StreamPath, removedSegment.FilePath);
        }

        private async Task WriteManifestAsync()
        {
            await _manifestWriter.WriteAsync(_config.ManifestOutputPath, _segments);
            _logger.HlsManifestUpdated(Name, ContextIdentifier, _config.ManifestOutputPath, StreamPath);
        }

        public async ValueTask ExecuteCleanupAsync()
        {
            if (!_config.CleanupDelay.HasValue)
                return;

            await _cleanupManager.ExecuteCleanupAsync(_config.ManifestOutputPath);
        }

        public async ValueTask ScheduleCleanupAsync()
        {
            if (!_config.CleanupDelay.HasValue)
                return;

            try
            {
                var segments = _segments.ToList();
                var cleanupDelay = CalculateCleanupDelay(segments, _config.CleanupDelay.Value);

                var files = new List<string> { _config.ManifestOutputPath };
                files.AddRange(segments.Select(x => x.FilePath));

                await _cleanupManager.ScheduleCleanupAsync(_config.ManifestOutputPath, files, cleanupDelay);
            }
            catch (Exception ex)
            {
                _logger.SchedulingHlsCleanupError(_config.ManifestOutputPath, ex);
            }
        }

        public ValueTask InterceptMediaPacketAsync(MediaType mediaType, IRentedBuffer buffer, uint timestamp)
        {
            return ValueTask.CompletedTask;
        }

        private static TimeSpan CalculateCleanupDelay(IList<TsSegment> tsSegments, TimeSpan cleanupDelay)
        {
            if (!tsSegments.Any())
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(tsSegments.Count * tsSegments.Max(x => x.Duration)) + cleanupDelay;
        }
    }
}
