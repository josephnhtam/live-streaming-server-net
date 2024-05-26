using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Containers;
using LiveStreamingServerNet.Transmuxer.Internal.Containers.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8.Marshal.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls
{
    internal partial class HlsTransmuxer : IHlsTransmuxer
    {
        private readonly IClientHandle _client;
        private readonly IHlsTransmuxerManager _transmuxerManager;
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
            IClientHandle client,
            IHlsTransmuxerManager transmuxerManager,
            IManifestWriter manifestWriter,
            ITsMuxer tsMuxer,
            Configuration config,
            ILogger<HlsTransmuxer> logger)
        {
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.ManifestOutputhPath));
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.TsFileOutputPath));

            Name = config.Name;
            ContextIdentifier = config.ContextIdentifier;

            _client = client;
            _transmuxerManager = transmuxerManager;
            _manifestWriter = manifestWriter;
            _tsMuxer = tsMuxer;

            _config = config;
            _logger = logger;

            _segments = new Queue<TsSegment>();
            _channel = Channel.CreateUnbounded<PendingMediaPacket>(new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = true });
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

            if (_tsMuxer.BufferSize > _config.MaxSegmentBufferSize)
                throw new OutOfMemoryException("Segment buffer size exceeded the maximum allowed size");
        }

        private async ValueTask ProcessVideoPacket(IRentedBuffer rentedBuffer, uint timestamp)
        {
            var tagHeader = FlvParser.ParseVideoTagHeader(rentedBuffer.Buffer);

            if (tagHeader.VideoCodec != VideoCodec.AVC)
                return;

            var dataBuffer = new ArraySegment<byte>(rentedBuffer.Buffer, FlvVideoTagHeader.Size, rentedBuffer.Size - FlvVideoTagHeader.Size);

            if (tagHeader.FrameType == VideoFrameType.KeyFrame)
                await TryToFlushAsync(timestamp);

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

        private ValueTask ProcessAudioPacket(IRentedBuffer rentedBuffer, uint timestamp)
        {
            var tagHeader = FlvParser.ParseAudioTagHeader(rentedBuffer.Buffer);

            if (tagHeader.AudioCodec != AudioCodec.AAC)
                return ValueTask.CompletedTask;

            var dataBuffer = new ArraySegment<byte>(rentedBuffer.Buffer, FlvAudioTagHeader.Size, rentedBuffer.Size - FlvAudioTagHeader.Size);

            if (tagHeader.AACPacketType == AACPacketType.SequenceHeader)
            {
                var sequenceHeader = AACParser.ParseSequenceHeader(dataBuffer);
                _tsMuxer.SetAACSequenceHeader(sequenceHeader);
                return ValueTask.CompletedTask;
            }

            _hasAudio |= _tsMuxer.WriteAudioPacket(dataBuffer, timestamp);
            return ValueTask.CompletedTask;
        }

        private async Task TryToFlushAsync(uint timestamp)
        {
            if (!_hasVideo)
                return;

            _hasVideo = _hasAudio = false;

            var tsSegment = await FlushTsMuxer(timestamp);
            if (tsSegment != null) await RefreshHlsAsync(tsSegment.Value);
        }

        private async ValueTask<TsSegment?> FlushTsMuxer(uint timestamp)
        {
            var tsSegment = await _tsMuxer.FlushAsync(timestamp);

            if (tsSegment.HasValue)
                _logger.TsSegmentCreated(Name, ContextIdentifier, _streamPath, tsSegment.Value.FilePath, tsSegment.Value.SequenceNumber, tsSegment.Value.Duration);

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
            await _manifestWriter.WriteAsync(_config.ManifestOutputhPath, _segments);
            _logger.HlsManifestUpdated(Name, ContextIdentifier, _config.ManifestOutputhPath, _streamPath);
        }

        public async Task RunAsync(
            string inputPath,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            OnTransmuxerStarted? onStarted,
            OnTransmuxerEnded? onEnded,
            CancellationToken cancellation)
        {
            _streamPath = streamPath;

            if (!_transmuxerManager.RegisterTransmuxer(streamPath, this))
            {
                _logger.RegisteringHlsTransmuxerFailed(streamPath);
                return;
            }

            try
            {
                _logger.HlsTransmuxerStarted(Name, ContextIdentifier, _config.ManifestOutputhPath, streamPath);

                onStarted?.Invoke(_config.ManifestOutputhPath);

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
                _logger.ProcessingHlsTransmuxingError(Name, ContextIdentifier, _config.ManifestOutputhPath, streamPath, ex);
                await _client.DisconnectAsync();
            }
            finally
            {
                _transmuxerManager.UnregisterTransmuxer(streamPath);
                onEnded?.Invoke(_config.ManifestOutputhPath);
                _tsMuxer?.Dispose();

                _logger.HlsTransmuxerEnded(Name, ContextIdentifier, _config.ManifestOutputhPath, streamPath);
            }

            while (_channel.Reader.TryRead(out var package))
            {
                package.RentedBuffer.Unclaim();
            }
        }

        private record struct PendingMediaPacket(MediaType MediaType, IRentedBuffer RentedBuffer, uint Timestamp);
    }
}
