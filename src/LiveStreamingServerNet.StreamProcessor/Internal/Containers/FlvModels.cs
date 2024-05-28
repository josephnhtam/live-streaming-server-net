using LiveStreamingServerNet.Rtmp;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
{
    internal record struct FlvVideoTagHeader(VideoFrameType FrameType, VideoCodec VideoCodec, AVCPacketType AVCPacketType, uint CompositionTime)
    {
        public const int Size = 5;
    }

    internal record struct FlvAudioTagHeader(AudioCodec AudioCodec, int SoundRate, int SoundSize, int SoundType, AACPacketType AACPacketType)
    {
        public const int Size = 2;
    }
}
