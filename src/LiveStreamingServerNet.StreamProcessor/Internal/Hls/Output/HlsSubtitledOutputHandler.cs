using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using static LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.HlsTransmuxer;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal class HlsSubtitledOutputHandler : IHlsOutputHandler
    {
        private readonly IMediaManifestWriter _manifestWriter;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly Configuration _config;
        private readonly ILogger<HlsSubtitledOutputHandler> _logger;
        private readonly Queue<SeqSegment> _segments;
        private readonly CancellationTokenSource _cts;
        private readonly List<ISubtitleTranscriber> _subtitleTranscribers;

        private Task? _transcriptionResultConsumingTask;

        public string Name { get; }
        public Guid ContextIdentifier { get; }
        public string StreamPath { get; }

        public HlsSubtitledOutputHandler(
            IDataBufferPool bufferPool,
            IMediaManifestWriter manifestWriter,
            IHlsCleanupManager cleanupManager,
            IReadOnlyList<SubtitleTranscriptionStreamFactory> subtitleStreamFactories,
            Configuration config,
            ILogger<HlsSubtitledOutputHandler> logger)
        {
            Name = config.TransmuxerName;
            ContextIdentifier = config.ContextIdentifier;
            StreamPath = config.StreamPath;

            _manifestWriter = manifestWriter;
            _cleanupManager = cleanupManager;
            _config = config;
            _logger = logger;

            _segments = new Queue<SeqSegment>();
            _cts = new CancellationTokenSource();

            var inputStreamWriterFactory = new FlvAudioStreamWriterFactory(bufferPool);
            _subtitleTranscribers = subtitleStreamFactories.Select(x =>
                new SubtitleTranscriber(x.Options, x.Factory.Create(inputStreamWriterFactory, x.Options)) as ISubtitleTranscriber
            ).ToList();
        }

        public async ValueTask InitializeAsync()
        {
            await Task.WhenAll(_subtitleTranscribers.Select(x => x.StartAsync().AsTask()));
            _transcriptionResultConsumingTask = ConsumeTranscriptionResultsAsync(_cts.Token);
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

        public async ValueTask InterceptMediaPacketAsync(MediaType mediaType, IRentedBuffer buffer, uint timestamp)
        {
            if (mediaType != MediaType.Audio)
            {
                return;
            }

            foreach (var transcriber in _subtitleTranscribers)
            {
                await transcriber.EnqueueAudioBufferAsync(buffer, timestamp);
            }
        }

        private async Task ConsumeTranscriptionResultsAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(_subtitleTranscribers.Select(x => ConsumeTranscriptionResultsAsync(x, cancellationToken)));
        }

        private async Task ConsumeTranscriptionResultsAsync(ISubtitleTranscriber transcriber, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await transcriber.ReceiveTranscriptionResultAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();

            foreach (var transcriber in _subtitleTranscribers)
            {
                await transcriber.DisposeAsync();
            }

            if (_transcriptionResultConsumingTask != null)
            {
                await _transcriptionResultConsumingTask;
            }
        }

        private static TimeSpan CalculateCleanupDelay(IList<SeqSegment> tsSegments, TimeSpan cleanupDelay)
        {
            if (!tsSegments.Any())
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(tsSegments.Count * tsSegments.Max(x => x.Duration)) + cleanupDelay;
        }
    }
}
