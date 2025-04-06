using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal partial class HlsSubtitledOutputHandler : IHlsOutputHandler
    {
        private readonly IMasterManifestWriter _masterManifestWriter;
        private readonly IMediaManifestWriter _mediaManifestWriter;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly Configuration _config;
        private readonly ILogger<HlsSubtitledOutputHandler> _logger;

        private readonly Queue<SeqSegment> _segments;
        private readonly CancellationTokenSource _cts;
        private readonly DateTime _initialProgramDateTime;
        private readonly IReadOnlyList<ISubtitleTranscriber> _subtitleTranscribers;
        private readonly ITargetDuration _targetDuration;

        private int _masterManifestCreated;

        public string Name { get; }
        public Guid ContextIdentifier { get; }
        public string StreamPath { get; }

        public HlsSubtitledOutputHandler(
            IDataBufferPool bufferPool,
            IMasterManifestWriter masterManifestWriter,
            IMediaManifestWriter mediaManifestWriter,
            IHlsCleanupManager cleanupManager,
            IReadOnlyList<ISubtitleTranscriber> subtitleTranscribers,
            DateTime initialProgramDateTime,
            Configuration config,
            ILogger<HlsSubtitledOutputHandler> logger)
        {
            Name = config.TransmuxerName;
            ContextIdentifier = config.ContextIdentifier;
            StreamPath = config.StreamPath;

            _masterManifestWriter = masterManifestWriter;
            _mediaManifestWriter = mediaManifestWriter;
            _cleanupManager = cleanupManager;
            _subtitleTranscribers = subtitleTranscribers;
            _initialProgramDateTime = initialProgramDateTime;
            _config = config;
            _logger = logger;

            _segments = new Queue<SeqSegment>();
            _cts = new CancellationTokenSource();
            _targetDuration = new MaximumTargetDuration();

            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.MasterManifestOutputPath));
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.MediaManifestOutputPath));
        }

        public async ValueTask InitializeAsync()
        {
            await Task.WhenAll(_subtitleTranscribers.Select(x => x.StartAsync().AsTask()));
        }

        public async ValueTask AddSegmentAsync(SeqSegment segment)
        {
            await DoAddSegmentAsync(segment);

            await WriteMediaManifestAsync();
            await WriteMasterManifestAsync();
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

        private async Task WriteMediaManifestAsync()
        {
            await _mediaManifestWriter.WriteAsync(_config.MediaManifestOutputPath, _segments, _targetDuration, _initialProgramDateTime);
            _logger.HlsManifestUpdated(Name, ContextIdentifier, StreamPath, _config.MediaManifestOutputPath);

            if (Interlocked.Exchange(ref _masterManifestCreated, 1) == 0)
            {
                var variantStreams = new List<VariantStream> { new(
                    _config.MediaManifestOutputPath,
                    ExtraAttributes: new Dictionary<string, string>{
                        ["SUBTITLES"] = "SUBS"
                    }
                ) };

                var alternateMedia = _subtitleTranscribers.Select(x =>
                    new AlternateMedia(
                        x.SubtitleManifestPath,
                        x.Options.Name ?? "DEFAULT",
                        "SUBTITLES",
                        "SUBS",
                        x.Options.Language,
                        x.Options.IsDefault,
                        x.Options.AutoSelect
                    )
                ).ToList();

                await _masterManifestWriter.WriteAsync(_config.MasterManifestOutputPath, variantStreams, alternateMedia);
                _logger.HlsManifestUpdated(Name, ContextIdentifier, StreamPath, _config.MasterManifestOutputPath);
            }
        }

        private async Task WriteMasterManifestAsync()
        {
            if (Interlocked.Exchange(ref _masterManifestCreated, 1) != 0)
                return;

            var variantStreams = new List<VariantStream> { new(
                    _config.MediaManifestOutputPath,
                    ExtraAttributes: new Dictionary<string, string>{
                        ["SUBTITLES"] = "SUBS"
                    }
                ) };

            var alternateMedia = _subtitleTranscribers.Select(x =>
                new AlternateMedia(
                    x.SubtitleManifestPath,
                    x.Options.Name ?? "DEFAULT",
                    "SUBTITLES",
                    "SUBS",
                    x.Options.Language,
                    x.Options.IsDefault,
                    x.Options.AutoSelect
                )
            ).ToList();

            await _masterManifestWriter.WriteAsync(_config.MasterManifestOutputPath, variantStreams, alternateMedia);
            _logger.HlsManifestUpdated(Name, ContextIdentifier, StreamPath, _config.MasterManifestOutputPath);
        }

        public async ValueTask ExecuteCleanupAsync()
        {
            if (!_config.CleanupDelay.HasValue)
                return;

            await _cleanupManager.ExecuteCleanupAsync(_config.MasterManifestOutputPath);
        }

        public async ValueTask ScheduleCleanupAsync()
        {
            if (!_config.CleanupDelay.HasValue)
                return;

            try
            {
                var segments = _segments.ToList();
                var cleanupDelay = CalculateCleanupDelay(segments, _config.CleanupDelay.Value);

                var files = new List<string> { _config.MasterManifestOutputPath };
                files.AddRange(segments.Select(x => x.FilePath));

                await _cleanupManager.ScheduleCleanupAsync(_config.MasterManifestOutputPath, files, cleanupDelay);
            }
            catch (Exception ex)
            {
                _logger.SchedulingHlsCleanupError(_config.MasterManifestOutputPath, ex);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();

            foreach (var transcriber in _subtitleTranscribers)
            {
                await transcriber.DisposeAsync();
            }
        }

        private static TimeSpan CalculateCleanupDelay(IList<SeqSegment> segments, TimeSpan cleanupDelay)
        {
            if (!segments.Any())
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(segments.Count * segments.Max(x => x.Duration)) + cleanupDelay;
        }
    }
}
