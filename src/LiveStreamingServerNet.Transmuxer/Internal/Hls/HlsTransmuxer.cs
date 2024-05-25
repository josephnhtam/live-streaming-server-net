using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Containers;
using LiveStreamingServerNet.Transmuxer.Internal.Containers.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8.Marshal.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls
{
    internal partial class HlsTransmuxer : IHlsTransmuxer
    {
        private readonly IHlsTransmuxerManager _transmuxerManager;
        private readonly IManifestWriter _manifestWriter;
        private readonly ITsMuxer _tsMuxer;

        private readonly Configuration _config;
        private readonly Queue<TsSegment> _segments;

        private bool _hasVideo;
        private bool _hasAudio;

        public string Name { get; }
        public Guid ContextIdentifier { get; }

        public HlsTransmuxer(IHlsTransmuxerManager transmuxerManager, IManifestWriter manifestWriter, ITsMuxer tsMuxer, Configuration config)
        {
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.ManifestOutputhPath));
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.TsFileOutputPath));

            Name = config.Name;
            ContextIdentifier = config.ContextIdentifier;

            _transmuxerManager = transmuxerManager;
            _manifestWriter = manifestWriter;
            _tsMuxer = tsMuxer;

            _config = config;
            _segments = new Queue<TsSegment>();
        }

        public ValueTask OnReceiveMediaMessage(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            rentedBuffer.Claim();

            try
            {
                switch (mediaType)
                {
                    case MediaType.Video:
                        return OnReceiveVideoMessage(rentedBuffer, timestamp);

                    case MediaType.Audio:
                        return OnReceiveAudioMessage(rentedBuffer, timestamp);
                }

                return ValueTask.CompletedTask;
            }
            finally
            {
                rentedBuffer.Unclaim();
            }
        }

        public async ValueTask OnReceiveVideoMessage(IRentedBuffer rentedBuffer, uint timestamp)
        {
            var tagHeader = FlvParser.ParseVideoTagHeader(rentedBuffer.Buffer);

            if (tagHeader.CodecId != VideoCodecId.AVC)
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

        public ValueTask OnReceiveAudioMessage(IRentedBuffer rentedBuffer, uint timestamp)
        {
            var tagHeader = FlvParser.ParseAudioTagHeader(rentedBuffer.Buffer);

            if (tagHeader.SoundFormat != AudioSoundFormat.AAC)
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

            var tsSegment = await _tsMuxer.FlushAsync(timestamp);
            if (tsSegment != null) await RefreshHlsAsync(tsSegment.Value);
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
                TsSegment removedSegment = _segments.Dequeue();

                if (_config.DeleteOutdatedSegments)
                {
                    // todo: delete removed segment
                }
            }

            return ValueTask.CompletedTask;
        }

        private Task WriteManifestAsync()
        {
            return _manifestWriter.WriteAsync(_config.ManifestOutputhPath, _segments);
        }

        public async Task RunAsync(
            string inputPath,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            OnTransmuxerStarted? onStarted,
            OnTransmuxerEnded? onEnded,
            CancellationToken cancellation)
        {
            try
            {
                if (!_transmuxerManager.RegisterTransmuxer(streamPath, this))
                {
                    // failed to register transmuxer
                    return;
                }

                onStarted?.Invoke(_config.ManifestOutputhPath);

                await Task.Delay(Timeout.InfiniteTimeSpan, cancellation);
            }
            catch { }
            finally
            {
                _transmuxerManager.UnregisterTransmuxer(streamPath);
                onEnded?.Invoke(_config.ManifestOutputhPath);

                _tsMuxer?.Dispose();
            }
        }
    }
}
