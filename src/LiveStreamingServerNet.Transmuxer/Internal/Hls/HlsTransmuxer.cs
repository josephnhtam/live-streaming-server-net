using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Containers;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls
{
    internal class HlsTransmuxer : IHlsTransmuxer
    {
        private readonly IHlsTransmuxerManager _transmuxerManager;

        private TsMuxer _tsMuxer;
        private bool _hasVideo;
        private bool _hasAudio;

        public string Name { get; }
        public Guid ContextIdentifier { get; }
        public string OutputPath { get; }

        public HlsTransmuxer(string name, Guid contextIdentifier, IHlsTransmuxerManager transmuxerManager, string manifestOutputhPath, string tsFileOutputPath)
        {
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(manifestOutputhPath));
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(tsFileOutputPath));

            _transmuxerManager = transmuxerManager;
            _tsMuxer = new TsMuxer(tsFileOutputPath);

            Name = name;
            ContextIdentifier = contextIdentifier;
            OutputPath = manifestOutputhPath;
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
                await FlushAsync();

            if (tagHeader.FrameType == VideoFrameType.KeyFrame && tagHeader.AvcPacketType == AVCPacketType.SequenceHeader)
            {
                var sequenceHeader = AvcParser.ParseSequenceHeader(dataBuffer);
                _tsMuxer.SetAvcSequenceHeader(sequenceHeader);
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
            if (_tsMuxer == null)
                return ValueTask.CompletedTask;

            var tagHeader = FlvParser.ParseAudioTagHeader(rentedBuffer.Buffer);

            if (tagHeader.SoundFormat != AudioSoundFormat.AAC)
                return ValueTask.CompletedTask;

            var dataBuffer = new ArraySegment<byte>(rentedBuffer.Buffer, FlvAudioTagHeader.Size, rentedBuffer.Size - FlvAudioTagHeader.Size);

            if (tagHeader.AACPacketType == AACPacketType.SequenceHeader)
            {
                var sequenceHeader = AacParser.ParseSequenceHeader(dataBuffer);
                _tsMuxer.SetAacSequenceHeader(sequenceHeader);
                return ValueTask.CompletedTask;
            }

            _hasAudio |= _tsMuxer.WriteAudioPacket(dataBuffer, timestamp);
            return ValueTask.CompletedTask;
        }

        private async Task FlushAsync()
        {
            if (!_hasVideo)
                return;

            await _tsMuxer.FlushAsync();
            _hasVideo = _hasAudio = false;
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

                onStarted?.Invoke(OutputPath);

                await Task.Delay(Timeout.InfiniteTimeSpan, cancellation);
            }
            catch { }
            finally
            {
                _transmuxerManager.UnregisterTransmuxer(streamPath);
                onEnded?.Invoke(OutputPath);

                _tsMuxer?.Dispose();
            }
        }
    }
}
