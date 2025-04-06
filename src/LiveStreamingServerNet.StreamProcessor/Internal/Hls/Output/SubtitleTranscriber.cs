using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.StreamProcessor.Transcriptions;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal partial class SubtitleTranscriber : ISubtitleTranscriber
    {
        private readonly SubtitleTranscriberConfiguration _config;
        private readonly ITranscriptionStream _stream;
        private readonly DateTime _initialProgramDateTime;
        private readonly ILogger _logger;

        private readonly IMediaManifestWriter _manifestWriter;
        private readonly IWebVttWriter _webVttWriter;

        private readonly CancellationTokenSource _cts;
        private readonly Channel<AudioBuffer> _audioBufferChannel;
        private readonly Channel<TranscriptionResult> _transcriptionResults;
        private readonly Queue<SeqSegment> _segments;
        private readonly TimeSpan _flushInterval;
        private readonly ITargetDuration _targetDuration;

        private uint _sequenceNumber;
        private Task? _audioPublishingTask;
        private Task? _transcriptionProcessingTask;

        public SubtitleTrackOptions Options { get; }
        public string SubtitleManifestPath => _config.SubtitleManifestOutputPath;

        public SubtitleTranscriber(
            SubtitleTrackOptions options,
            SubtitleTranscriberConfiguration config,
            ITranscriptionStream stream,
            DateTime initialProgramDateTime,
            ILogger<SubtitleTranscriber> logger)
        {
            Options = options;

            _config = config;
            _stream = stream;
            _initialProgramDateTime = initialProgramDateTime;
            _logger = logger;

            _manifestWriter = new MediaManifestWriter();
            _webVttWriter = new WebVttWriter();

            _cts = new CancellationTokenSource();
            _audioBufferChannel = Channel.CreateUnbounded<AudioBuffer>(new() { AllowSynchronousContinuations = true });
            _transcriptionResults = Channel.CreateUnbounded<TranscriptionResult>(new() { AllowSynchronousContinuations = true });
            _segments = new Queue<SeqSegment>();

            _flushInterval = TimeSpan.FromSeconds(1);
            _targetDuration = new FixedTargetDuration(_flushInterval);

            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(_config.SubtitleManifestOutputPath));
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(_config.SubtitleSegmentOutputPath));

            ConfigureTranscriptionEvents();
        }

        private void ConfigureTranscriptionEvents()
        {
            _stream.TranscriptionResultReceived += (_, e) =>
                ErrorBoundary.Execute(() => _transcriptionResults.Writer.TryWrite(e.Result));
        }

        public async ValueTask StartAsync()
        {
            _logger.SubtitleTranscriberStarted(_config.TransmuxerName, _config.ContextIdentifier, _config.StreamPath);

            await WriteManifestAsync(Enumerable.Empty<SeqSegment>(), CancellationToken.None);
            await _stream.StartAsync();

            _audioPublishingTask = PublishAudioBufferAsync(_cts.Token);
            _transcriptionProcessingTask = ProcessTranscriptionResultsAsync(_cts.Token);
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

        public ValueTask<TranscriptionResult> ReceiveTranscriptionResultAsync(CancellationToken cancellationToken)
        {
            return _transcriptionResults.Reader.ReadAsync(cancellationToken);
        }

        private async Task ProcessTranscriptionResultsAsync(CancellationToken cancellationToken)
        {
            _logger.TranscriptionProcessingStarted(_config.TransmuxerName, _config.ContextIdentifier, _config.StreamPath);

            var segmentStart = TimeSpan.Zero;
            var cues = new List<SubtitleCue>();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(_flushInterval, cancellationToken);

                    if (!TryExtractSubtitleCues(segmentStart, ref cues, out var segmentEnd))
                        continue;

                    var segment = await CreateWebVttSegment(++_sequenceNumber, segmentStart, cues, segmentEnd, cancellationToken);
                    _segments.Enqueue(segment);

                    await WriteManifestAsync(_segments.ToList(), cancellationToken);
                    segmentStart = segmentEnd;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.TranscriptionProcessingError(_config.TransmuxerName, _config.ContextIdentifier, _config.StreamPath, ex);
            }
            finally
            {
                _transcriptionResults.Writer.Complete();
            }
        }

        private async Task<SeqSegment> CreateWebVttSegment(
            uint sequenceNumber, TimeSpan segmentStart, List<SubtitleCue> cues, TimeSpan segmentEnd, CancellationToken cancellationToken)
        {
            var outputPath = _config.SubtitleSegmentOutputPath.Replace("{seqNum}", sequenceNumber.ToString());
            await _webVttWriter.WriteAsync(outputPath, cues, cancellationToken);

            var timestamp = (uint)segmentStart.TotalMilliseconds;
            var duration = (uint)(segmentEnd - segmentStart).TotalMilliseconds;

            var segment = new SeqSegment(outputPath, sequenceNumber, timestamp, duration);

            _logger.SubtitleSegmentCreated(
                _config.TransmuxerName, _config.ContextIdentifier, _config.StreamPath,
                _sequenceNumber, segment.FilePath, segment.Timestamp, segment.Duration);

            return segment;
        }

        private async Task WriteManifestAsync(IEnumerable<SeqSegment> segments, CancellationToken cancellationToken)
        {
            await _manifestWriter.WriteAsync(_config.SubtitleManifestOutputPath, segments, _targetDuration, _initialProgramDateTime, cancellationToken);
        }

        private bool TryExtractSubtitleCues(TimeSpan segmentStart, ref List<SubtitleCue> cues, out TimeSpan segmentEnd)
        {
            cues.Clear();
            var nextCueTimestamp = segmentStart;

            while (_transcriptionResults.Reader.TryRead(out var transcription))
            {
                var transcriptionEnd = transcription.Timestamp + transcription.Duration;
                var cueStart = transcription.Timestamp > nextCueTimestamp ? transcription.Timestamp : nextCueTimestamp;
                var cueDuration = transcriptionEnd - cueStart;

                if (cueDuration <= TimeSpan.Zero)
                    continue;

                cues.Add(new SubtitleCue(transcription.Text, cueStart, cueDuration));
                nextCueTimestamp = cueStart + cueDuration;
            }

            segmentEnd = nextCueTimestamp;
            return cues.Count > 0;
        }

        private async Task PublishAudioBufferAsync(CancellationToken cancellationToken)
        {
            _logger.AudioPublishingStarted(_config.TransmuxerName, _config.ContextIdentifier, _config.StreamPath);

            try
            {
                await foreach (var audioBuffer in _audioBufferChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        await _stream.SendAsync(audioBuffer.Buffer, audioBuffer.Timestamp, cancellationToken);
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
                _logger.AudioPublishingError(_config.TransmuxerName, _config.ContextIdentifier, _config.StreamPath, ex);
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
            _logger.SubtitleTranscriberStopping(_config.TransmuxerName, _config.ContextIdentifier, _config.StreamPath);
            _cts.Cancel();

            if (_audioPublishingTask != null)
            {
                await _audioPublishingTask;
            }

            if (_transcriptionProcessingTask != null)
            {
                await _transcriptionProcessingTask;
            }

            await _stream.DisposeAsync();
        }

        private record struct AudioBuffer(IRentedBuffer Buffer, uint Timestamp);
    }
}