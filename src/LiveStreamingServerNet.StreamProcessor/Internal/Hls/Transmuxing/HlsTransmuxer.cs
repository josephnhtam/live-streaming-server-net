using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Utilities.Containers;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Contracts;
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
        private readonly ISessionHandle _client;
        private readonly IHlsTransmuxerManager _transmuxerManager;
        private readonly IHlsOutputHandler _outputHandler;
        private readonly IHlsPathRegistry _pathRegistry;
        private readonly ITsMuxer _tsMuxer;

        private readonly Configuration _config;
        private readonly ILogger _logger;

        private readonly Channel<PendingMediaPacket> _channel;

        private bool _hasVideo;
        private bool _hasAudio;

        private bool _registeredHlsOutputPath;

        public string Name { get; }
        public Guid ContextIdentifier { get; }
        public string StreamPath { get; }

        public HlsTransmuxer(
            ISessionHandle client,
            IHlsTransmuxerManager transmuxerManager,
            IHlsOutputHandler outputHandler,
            IHlsPathRegistry pathRegistry,
            ITsMuxer tsMuxer,
            Configuration config,
            ILogger<HlsTransmuxer> logger)
        {
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.ManifestOutputPath));

            Name = config.TransmuxerName;
            ContextIdentifier = config.ContextIdentifier;
            StreamPath = config.StreamPath;

            _client = client;
            _transmuxerManager = transmuxerManager;
            _outputHandler = outputHandler;
            _pathRegistry = pathRegistry;
            _tsMuxer = tsMuxer;

            _config = config;
            _logger = logger;

            _channel = Channel.CreateUnbounded<PendingMediaPacket>(new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = true });

            if (!_transmuxerManager.RegisterTransmuxer(StreamPath, this))
            {
                _logger.RegisteringHlsTransmuxerFailed(StreamPath);
                throw new InvalidOperationException("A HLS transmuxer of the same stream path is already registered");
            }
        }

        public ValueTask AddMediaPacketAsync(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            rentedBuffer.Claim();

            try
            {
                if (!_channel.Writer.TryWrite(new PendingMediaPacket(mediaType, rentedBuffer, timestamp)))
                {
                    throw new ChannelClosedException();
                }
            }
            catch
            {
                rentedBuffer.Unclaim();
            }

            return ValueTask.CompletedTask;
        }

        private async ValueTask ProcessMediaPacketAsync(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            await _outputHandler.InterceptMediaPacketAsync(mediaType, rentedBuffer, timestamp);

            switch (mediaType)
            {
                case MediaType.Video:
                    await ProcessVideoPacketAsync(rentedBuffer, timestamp);
                    break;

                case MediaType.Audio:
                    await ProcessAudioPacketAsync(rentedBuffer, timestamp);
                    break;
            }

            if (_tsMuxer.PayloadSize > _config.MaxSegmentSize)
                throw new ArgumentOutOfRangeException("Segment size exceeded the maximum allowed size");

            if (_tsMuxer.BufferSize > _config.MaxSegmentBufferSize)
                await FlushTsMuxerPartiallyAsync();
        }

        private async ValueTask ProcessVideoPacketAsync(IRentedBuffer rentedBuffer, uint timestamp)
        {
            var tagHeader = FlvParser.ParseVideoTagHeader(rentedBuffer.Buffer);

            if (tagHeader.VideoCodec is not (VideoCodec.AVC or VideoCodec.HEVC))
                return;

            await TryToFlushAsync(tagHeader.FrameType == VideoFrameType.KeyFrame, timestamp);

            var dataBuffer = new ArraySegment<byte>(rentedBuffer.Buffer, FlvVideoTagHeader.Size, rentedBuffer.Size - FlvVideoTagHeader.Size);

            if (tagHeader.FrameType == VideoFrameType.KeyFrame && tagHeader.AVCPacketType == AVCPacketType.SequenceHeader)
            {
                if (tagHeader.VideoCodec == VideoCodec.AVC)
                {
                    var sequenceHeader = AVCParser.ParseSequenceHeader(dataBuffer);
                    _tsMuxer.SetAVCSequenceHeader(sequenceHeader);
                }
                else if (tagHeader.VideoCodec == VideoCodec.HEVC)
                {
                    var sequenceHeader = AVCParser.ParseHEVCSequenceHeader(dataBuffer);
                    _tsMuxer.SetHEVCSequenceHeader(sequenceHeader);
                }

                return;
            }

            _hasVideo |= _tsMuxer.WriteVideoPacket(
                dataBuffer,
                timestamp,
                tagHeader.CompositionTime,
                tagHeader.FrameType == VideoFrameType.KeyFrame
            );
        }

        private async ValueTask ProcessAudioPacketAsync(IRentedBuffer rentedBuffer, uint timestamp)
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

        private async ValueTask<bool> TryToFlushAsync(bool isKeyFrame, uint timestamp)
        {
            if (ShouldFlush(isKeyFrame, timestamp))
            {
                _hasVideo = _hasAudio = false;

                var tsSegment = await FlushTsMuxerAsync(timestamp);

                if (tsSegment.HasValue)
                {
                    await _outputHandler.AddSegmentAsync(tsSegment.Value);
                }

                return true;
            }

            return false;

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

        private async ValueTask FlushTsMuxerPartiallyAsync()
        {
            var tsSegmentPartial = await _tsMuxer.FlushPartialAsync();

            if (tsSegmentPartial.HasValue)
                _logger.TsSegmentFlushedPartially(Name, ContextIdentifier, StreamPath, tsSegmentPartial.Value.FilePath, tsSegmentPartial.Value.SequenceNumber);
        }

        private async ValueTask<SeqSegment?> FlushTsMuxerAsync(uint timestamp)
        {
            var tsSegment = await _tsMuxer.FlushAsync(timestamp);

            if (tsSegment.HasValue)
                _logger.TsSegmentFlushed(Name, ContextIdentifier, StreamPath, tsSegment.Value.FilePath, tsSegment.Value.SequenceNumber, tsSegment.Value.Duration);

            return tsSegment;
        }

        public async Task RunAsync(
            string inputPath,
            IReadOnlyDictionary<string, string> streamArguments,
            OnStreamProcessorStarted? onStarted,
            OnStreamProcessorEnded? onEnded,
            CancellationToken cancellation)
        {
            try
            {
                await PreRunAsync();

                _logger.HlsTransmuxerStarted(Name, ContextIdentifier, _config.ManifestOutputPath, StreamPath);

                onStarted?.Invoke(_config.ManifestOutputPath);

                while (!cancellation.IsCancellationRequested)
                {
                    var message = await _channel.Reader.ReadAsync(cancellation);

                    try
                    {
                        await ProcessMediaPacketAsync(message.MediaType, message.RentedBuffer, message.Timestamp);
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
                _logger.ProcessingHlsTransmuxingError(Name, ContextIdentifier, _config.ManifestOutputPath, StreamPath, ex);
                await _client.DisconnectAsync();
            }
            finally
            {
                ChannelCleanup();

                await PostRunAsync();
                await _outputHandler.DisposeAsync();

                _transmuxerManager.UnregisterTransmuxer(StreamPath);
                onEnded?.Invoke(_config.ManifestOutputPath);
                _tsMuxer?.Dispose();

                _logger.HlsTransmuxerEnded(Name, ContextIdentifier, _config.ManifestOutputPath, StreamPath);
            }
        }

        private void ChannelCleanup()
        {
            _channel.Writer.Complete();

            while (_channel.Reader.TryRead(out var packet))
            {
                packet.RentedBuffer.Unclaim();
            }
        }

        private async ValueTask PreRunAsync()
        {
            RegisterHlsOutputPath();
            await _outputHandler.InitializeAsync();
            await _outputHandler.ExecuteCleanupAsync();
        }

        private async ValueTask PostRunAsync()
        {
            await _outputHandler.ScheduleCleanupAsync();
            UnregisterHlsOutputPath();
        }

        private void RegisterHlsOutputPath()
        {
            var outputPath = Path.GetDirectoryName(_config.ManifestOutputPath) ?? string.Empty;

            if (!_pathRegistry.RegisterHlsOutputPath(StreamPath, outputPath))
                throw new InvalidOperationException("A HLS output path of the same stream path is already registered");

            _registeredHlsOutputPath = true;
        }

        private void UnregisterHlsOutputPath()
        {
            if (!_registeredHlsOutputPath)
                return;

            _pathRegistry.UnregisterHlsOutputPath(StreamPath);
            _registeredHlsOutputPath = false;
        }

        private record struct PendingMediaPacket(MediaType MediaType, IRentedBuffer RentedBuffer, uint Timestamp);
    }
}
