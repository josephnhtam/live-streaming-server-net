using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal partial class HlsOutputHandler : IHlsOutputHandler
    {
        private readonly IMediaManifestWriter _manifestWriter;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly Configuration _config;
        private readonly ILogger<HlsOutputHandler> _logger;
        private readonly Queue<SeqSegment> _segments;
        private readonly ITargetDuration _targetDuration;

        public string Name { get; }
        public Guid ContextIdentifier { get; }
        public string StreamPath { get; }

        public HlsOutputHandler(
            IDataBufferPool bufferPool,
            IMediaManifestWriter manifestWriter,
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
            _segments = new Queue<SeqSegment>();
            _targetDuration = new MaximumTargetDuration();
        }

        public ValueTask InitializeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public async ValueTask AddSegmentAsync(SeqSegment segment)
        {
            await DoAddSegmentAsync(segment);
            await WriteManifestAsync();
        }

        private ValueTask DoAddSegmentAsync(SeqSegment segment)
        {
            _segments.Enqueue(segment);

            if (_segments.Count > _config.SegmentListSize)
            {
                var removedSegment = _segments.Dequeue();

                if (_config.DeleteOutdatedSegments)
                    DeleteOutdatedSegment(removedSegment);
            }

            return ValueTask.CompletedTask;
        }

        private void DeleteOutdatedSegment(SeqSegment removedSegment)
        {
            File.Delete(removedSegment.FilePath);
            _logger.OutdatedSegmentDeleted(Name, ContextIdentifier, StreamPath, removedSegment.FilePath);
        }

        private async Task WriteManifestAsync()
        {
            await _manifestWriter.WriteAsync(_config.ManifestOutputPath, _segments, _targetDuration, null);
            _logger.HlsManifestUpdated(Name, ContextIdentifier, StreamPath, _config.ManifestOutputPath);
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

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        private static TimeSpan CalculateCleanupDelay(IList<SeqSegment> segments, TimeSpan cleanupDelay)
        {
            if (!segments.Any())
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(segments.Count * segments.Max(x => x.Duration)) + cleanupDelay;
        }
    }
}
