using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.M3u8.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing
{
    internal partial class HlsTransmuxer : IHlsTransmuxer
    {
        private readonly IClientHandle _client;
        private readonly IHlsTransmuxerManager _transmuxerManager;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly IManifestWriter _manifestWriter;
        private readonly ITsMuxer _tsMuxer;

        private readonly Configuration _config;
        private readonly ILogger _logger;

        private readonly Queue<TsSegment> _segments;
        private readonly Channel<PendingMediaPacket> _channel;

        private bool _hasVideo;
        private bool _hasAudio;

        private string _streamPath = string.Empty;

        public string Name { get; }
        public Guid ContextIdentifier { get; }

        public HlsTransmuxer(
            string streamPath,
            IClientHandle client,
            IHlsTransmuxerManager transmuxerManager,
            IHlsCleanupManager cleanupManager,
            IManifestWriter manifestWriter,
            ITsMuxer tsMuxer,
            Configuration config,
            ILogger<HlsTransmuxer> logger)
        {
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.ManifestOutputPath));
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.TsSegmentOutputPath));

            Name = config.TransmuxerName;
            ContextIdentifier = config.ContextIdentifier;

            _client = client;
            _transmuxerManager = transmuxerManager;
            _cleanupManager = cleanupManager;
            _manifestWriter = manifestWriter;
            _tsMuxer = tsMuxer;

            _config = config;
            _logger = logger;

            _segments = new Queue<TsSegment>();
            _channel = Channel.CreateUnbounded<PendingMediaPacket>(new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = true });

            if (!_transmuxerManager.RegisterTransmuxer(streamPath, this))
            {
                _logger.RegisteringHlsTransmuxerFailed(streamPath);
                throw new InvalidOperationException("A HLS transmuxer of the same stream path is already registered");
            }
        }

        public ValueTask AddMediaPacket(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            rentedBuffer.Claim();
            return _channel.Writer.WriteAsync(new PendingMediaPacket(mediaType, rentedBuffer, timestamp));
        }

        private async ValueTask ProcessMediaPacket(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            switch (mediaType)
            {
                case MediaType.Video:
                    await ProcessVideoPacket(rentedBuffer, timestamp);
                    break;

                case MediaType.Audio:
                    await ProcessAudioPacket(rentedBuffer, timestamp);
                    break;
            }

            if (_tsMuxer.PayloadSize > _config.MaxSegmentSize)
                throw new ArgumentOutOfRangeException("Segment size exceeded the maximum allowed size");

            if (_tsMuxer.BufferSize > _config.MaxSegmentBufferSize)
                await FlushTsMuxerPartially();
        }

        private async ValueTask ProcessVideoPacket(IRentedBuffer rentedBuffer, uint timestamp)
        {
            var tagHeader = FlvParser.ParseVideoTagHeader(rentedBuffer.Buffer);

            if (tagHeader.VideoCodec != VideoCodec.AVC)
                return;

            await TryToFlushAsync(tagHeader.FrameType == VideoFrameType.KeyFrame, timestamp);

            var dataBuffer = new ArraySegment<byte>(rentedBuffer.Buffer, FlvVideoTagHeader.Size, rentedBuffer.Size - FlvVideoTagHeader.Size);

            if (tagHeader.FrameType == VideoFrameType.KeyFrame && tagHeader.AVCPacketType == AVCPacketType.SequenceHeader)
            {
                var sequenceHeader = AVCParser.ParseSequenceHeader(dataBuffer);
                _tsMuxer.SetAVCSequenceHeader(sequenceHeader);
                return;
            }

            _hasVideo |= _tsMuxer.WriteVideoPacket(
                dataBuffer,
                timestamp,
                tagHeader.CompositionTime,
                tagHeader.FrameType == VideoFrameType.KeyFrame
            );
        }

        private async ValueTask ProcessAudioPacket(IRentedBuffer rentedBuffer, uint timestamp)
        {
            var tagHeader = FlvParser.ParseAudioTagHeader(rentedBuffer.Buffer);

            if (tagHeader.AudioCodec != AudioCodec.AAC)
                return;

            await TryToFlushAsync(false, timestamp);

            var dataBuffer = new ArraySegment<byte>(rentedBuffer.Buffer, FlvAudioTagHeader.Size, rentedBuffer.Size - FlvAudioTagHeader.Size);

            if (tagHeader.AACPacketType == AACPacketType.SequenceHeader)
            {
                var sequenceHeader = AACParser.ParseSequenceHeader(dataBuffer);
                _tsMuxer.SetAACSequenceHeader(sequenceHeader);
                return;
            }

            _hasAudio |= _tsMuxer.WriteAudioPacket(dataBuffer, timestamp);
        }

        private async ValueTask TryToFlushAsync(bool isKeyFrame, uint timestamp)
        {
            if (ShouldFlush(isKeyFrame, timestamp))
            {
                _hasVideo = _hasAudio = false;

                var tsSegment = await FlushTsMuxer(timestamp);
                if (tsSegment != null) await RefreshHlsAsync(tsSegment.Value);
            }

            bool ShouldFlush(bool isKeyFrame, uint timestamp)
            {
                if (isKeyFrame && _hasVideo && _tsMuxer.SegmentTimestamp.HasValue &&
                    (timestamp - _tsMuxer.SegmentTimestamp.Value) >= _config.MinSegmentLength.TotalMilliseconds)
                {
                    return true;
                }

                if (!_hasVideo && _hasAudio && _tsMuxer.SegmentTimestamp.HasValue &&
                    (timestamp - _tsMuxer.SegmentTimestamp.Value) >= _config.AudioOnlySegmentLength.TotalMilliseconds)
                {
                    return true;
                }

                return false;
            }
        }

        private async ValueTask FlushTsMuxerPartially()
        {
            var tsSegmentPartial = await _tsMuxer.FlushPartialAsync();

            if (tsSegmentPartial.HasValue)
                _logger.TsSegmentFlushedPartially(Name, ContextIdentifier, _streamPath, tsSegmentPartial.Value.FilePath, tsSegmentPartial.Value.SequenceNumber);
        }

        private async ValueTask<TsSegment?> FlushTsMuxer(uint timestamp)
        {
            var tsSegment = await _tsMuxer.FlushAsync(timestamp);

            if (tsSegment.HasValue)
                _logger.TsSegmentFlushed(Name, ContextIdentifier, _streamPath, tsSegment.Value.FilePath, tsSegment.Value.SequenceNumber, tsSegment.Value.Duration);

            return tsSegment;
        }

        private async ValueTask RefreshHlsAsync(TsSegment newSegment)
        {
            await AddSegmentsAsync(newSegment);
            await WriteManifestAsync();
        }

        private ValueTask AddSegmentsAsync(TsSegment newSegment)
        {
            _segments.Enqueue(newSegment);

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
            _logger.OutdatedTsSegmentDeleted(Name, ContextIdentifier, _streamPath, removedSegment.FilePath);
        }

        private async Task WriteManifestAsync()
        {
            await _manifestWriter.WriteAsync(_config.ManifestOutputPath, _segments);
            _logger.HlsManifestUpdated(Name, ContextIdentifier, _config.ManifestOutputPath, _streamPath);
        }

        public async Task RunAsync(
            string inputPath,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            OnStreamProcessorStarted? onStarted,
            OnStreamProcessorEnded? onEnded,
            CancellationToken cancellation)
        {
            _streamPath = streamPath;

            try
            {
                await PreRunAsync();

                _logger.HlsTransmuxerStarted(Name, ContextIdentifier, _config.ManifestOutputPath, streamPath);

                onStarted?.Invoke(_config.ManifestOutputPath);

                while (!cancellation.IsCancellationRequested)
                {
                    var message = await _channel.Reader.ReadAsync(cancellation);

                    try
                    {
                        await ProcessMediaPacket(message.MediaType, message.RentedBuffer, message.Timestamp);
                    }
                    finally
                    {
                        message.RentedBuffer.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.ProcessingHlsTransmuxingError(Name, ContextIdentifier, _config.ManifestOutputPath, streamPath, ex);
                await _client.DisconnectAsync();
            }
            finally
            {
                await PostRunAsync();

                _transmuxerManager.UnregisterTransmuxer(streamPath);
                onEnded?.Invoke(_config.ManifestOutputPath);
                _tsMuxer?.Dispose();

                _logger.HlsTransmuxerEnded(Name, ContextIdentifier, _config.ManifestOutputPath, streamPath);
            }

            while (_channel.Reader.TryRead(out var package))
            {
                package.RentedBuffer.Unclaim();
            }
        }

        private async ValueTask PreRunAsync()
        {
            await ExecuteCleanupAsync();
        }

        private async ValueTask PostRunAsync()
        {
            await ScheduleCleanupAsync();
        }

        private async Task ExecuteCleanupAsync()
        {
            if (!_config.CleanupDelay.HasValue)
                return;

            await _cleanupManager.ExecuteCleanupAsync(_config.ManifestOutputPath);
        }

        private async ValueTask ScheduleCleanupAsync()
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

        private static TimeSpan CalculateCleanupDelay(IList<TsSegment> tsSegments, TimeSpan cleanupDelay)
        {
            if (!tsSegments.Any())
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(tsSegments.Count * tsSegments.Max(x => x.Duration)) + cleanupDelay;
        }

        private record struct PendingMediaPacket(MediaType MediaType, IRentedBuffer RentedBuffer, uint Timestamp);
    }
}
