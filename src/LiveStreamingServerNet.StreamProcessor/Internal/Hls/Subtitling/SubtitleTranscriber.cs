using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Writers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling
{
    internal partial class SubtitleTranscriber : ISubtitleTranscriber
    {
        private readonly SubtitleTranscriberConfiguration _config;
        private readonly ITranscriptionStream _stream;
        private readonly ISubtitleCueExtractor _subtitleCueExtractor;
        private readonly DateTime _initialProgramDateTime;
        private readonly ILogger _logger;

        private readonly IMediaManifestWriter _manifestWriter;
        private readonly IWebVttWriter _webVttWriter;

        private readonly CancellationTokenSource _cts;
        private readonly Channel<AudioBuffer> _audioBufferChannel;
        private readonly ConcurrentQueue<SeqSegment> _segments;
        private readonly TimeSpan _flushInterval;
        private readonly ITargetDuration _targetDuration;

        private uint _sequenceNumber;
        private Task? _audioPublishingTask;
        private Task? _transcriptionProcessingTask;

        public string Name { get; }
        public Guid ContextIdentifier { get; }
        public string StreamPath { get; }
        public SubtitleTrackOptions Options { get; }
        public string SubtitleManifestPath => _config.SubtitleManifestOutputPath;

        public SubtitleTranscriber(
            SubtitleTrackOptions options,
            SubtitleTranscriberConfiguration config,
            ITranscriptionStream stream,
            ISubtitleCueExtractor subtitleCueExtractor,
            DateTime initialProgramDateTime,
            ILogger<SubtitleTranscriber> logger)
        {
            Name = config.TransmuxerName;
            ContextIdentifier = config.ContextIdentifier;
            StreamPath = config.StreamPath;
            Options = options;

            _config = config;
            _stream = stream;
            _subtitleCueExtractor = subtitleCueExtractor;
            _initialProgramDateTime = initialProgramDateTime;
            _logger = logger;

            _manifestWriter = new MediaManifestWriter();
            _webVttWriter = new WebVttWriter();

            _cts = new CancellationTokenSource();
            _audioBufferChannel = Channel.CreateUnbounded<AudioBuffer>(new() { AllowSynchronousContinuations = true });
            _segments = new ConcurrentQueue<SeqSegment>();

            _flushInterval = TimeSpan.FromSeconds(1);
            _targetDuration = new FixedTargetDuration(_flushInterval);

            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(_config.SubtitleManifestOutputPath));
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(_config.SubtitleSegmentOutputPath));

            ConfigureTranscriptionEvents();
        }

        private void ConfigureTranscriptionEvents()
        {
            if (_subtitleCueExtractor.RequireTranscribingResult)
            {
                _stream.TranscribingResultReceived += (_, e) =>
                    _subtitleCueExtractor.ReceiveTranscribingResult(e.Result);
            }

            if (_subtitleCueExtractor.RequireTranscribedResult)
            {
                _stream.TranscribedResultReceived += (_, e) =>
                    _subtitleCueExtractor.ReceiveTranscribedResult(e.Result);
            }
        }

        public async ValueTask StartAsync()
        {
            _logger.SubtitleTranscriberStarted(Name, ContextIdentifier, StreamPath);

            await WriteManifestAsync(Enumerable.Empty<SeqSegment>(), CancellationToken.None);
            await _stream.StartAsync();

            _audioPublishingTask = PublishAudioBufferAsync(_cts.Token);
            _transcriptionProcessingTask = ProcessTranscriptionResultsAsync(_cts.Token);
        }

        public async ValueTask StopAsync()
        {
            _logger.SubtitleTranscriberStopping(Name, ContextIdentifier, StreamPath);

            _cts.Cancel();

            if (_audioPublishingTask != null)
            {
                await _audioPublishingTask;
            }

            if (_transcriptionProcessingTask != null)
            {
                await _transcriptionProcessingTask;
            }

            _logger.SubtitleTranscriberStopped(Name, ContextIdentifier, StreamPath);
        }

        public async ValueTask EnqueueAudioBufferAsync(IRentedBuffer rentedBuffer, uint timestamp)
        {
            rentedBuffer.Claim();

            try
            {
                await _audioBufferChannel.Writer.WriteAsync(new AudioBuffer(rentedBuffer, timestamp));
            }
            catch (Exception)
            {
                rentedBuffer.Unclaim();
            }
        }

        private async Task ProcessTranscriptionResultsAsync(CancellationToken cancellationToken)
        {
            _logger.TranscriptionProcessingStarted(Name, ContextIdentifier, StreamPath);

            var segmentStart = TimeSpan.Zero;
            var cues = new List<SubtitleCue>();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(_flushInterval, cancellationToken);

                    if (!_subtitleCueExtractor.TryExtractSubtitleCues(segmentStart, ref cues, out var segmentEnd))
                    {
                        continue;
                    }

                    var segment = await CreateWebVttSegment(++_sequenceNumber, segmentStart, cues, segmentEnd, cancellationToken);
                    _segments.Enqueue(segment);

                    await WriteManifestAsync(_segments.ToList(), cancellationToken);
                    segmentStart = segmentEnd;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.TranscriptionProcessingError(Name, ContextIdentifier, StreamPath, ex);
            }
        }

        private async Task<SeqSegment> CreateWebVttSegment(
            uint sequenceNumber, TimeSpan segmentStart, IReadOnlyList<SubtitleCue> cues, TimeSpan segmentEnd, CancellationToken cancellationToken)
        {
            var outputPath = GetSegmentOutputPath(sequenceNumber);
            await _webVttWriter.WriteAsync(outputPath, cues, cancellationToken);

            var timestamp = (uint)segmentStart.TotalMilliseconds;
            var duration = (uint)(segmentEnd - segmentStart).TotalMilliseconds;

            var segment = new SeqSegment(outputPath, sequenceNumber, timestamp, duration);

            _logger.SubtitleSegmentCreated(
                Name, ContextIdentifier, StreamPath,
                _sequenceNumber, segment.FilePath, segment.Timestamp, segment.Duration);

            return segment;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetSegmentOutputPath(uint sequenceNumber)
        {
            return _config.SubtitleSegmentOutputPath.Replace("{seqNum}", sequenceNumber.ToString());
        }

        private async Task WriteManifestAsync(IEnumerable<SeqSegment> segments, CancellationToken cancellationToken)
        {
            await _manifestWriter.WriteAsync(_config.SubtitleManifestOutputPath, segments, _targetDuration, _initialProgramDateTime, cancellationToken);
        }

        private async Task PublishAudioBufferAsync(CancellationToken cancellationToken)
        {
            _logger.AudioPublishingStarted(Name, ContextIdentifier, StreamPath);

            try
            {
                await foreach (var audioBuffer in _audioBufferChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        await _stream.WriteAsync(audioBuffer.Buffer, audioBuffer.Timestamp, cancellationToken);
                    }
                    finally
                    {
                        audioBuffer.Buffer.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.AudioPublishingError(Name, ContextIdentifier, StreamPath, ex);
            }
            finally
            {
                _audioBufferChannel.Writer.Complete();

                while (_audioBufferChannel.Reader.TryRead(out var audioBuffer))
                {
                    audioBuffer.Buffer.Unclaim();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _stream.DisposeAsync();
            await _subtitleCueExtractor.DisposeAsync();
        }

        public ValueTask ClearExpiredSegmentsAsync(uint oldestTimestamp)
        {
            while (_segments.TryPeek(out var segment) &&
                IsSegmentOutdated(segment, oldestTimestamp) &&
                _segments.TryDequeue(out var removedSegment) &&
                _config.DeleteOutdatedSegments)
            {
                DeleteOutdatedSegment(removedSegment);
            }

            return ValueTask.CompletedTask;
        }

        public List<SeqSegment> GetSegments()
        {
            return _segments.ToList();
        }

        private void DeleteOutdatedSegment(SeqSegment removedSegment)
        {
            File.Delete(removedSegment.FilePath);
            _logger.OutdatedSegmentDeleted(Name, ContextIdentifier, StreamPath, removedSegment.FilePath);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSegmentOutdated(SeqSegment segment, uint oldestTimestamp)
        {
            return segment.Timestamp + segment.Duration < oldestTimestamp;
        }

        private record struct AudioBuffer(IRentedBuffer Buffer, uint Timestamp);
    }
}