namespace LiveStreamingServerNet.Rtmp.Utilities.Containers
{
    public record struct FlvVideoTagHeader(VideoFrameType FrameType, VideoCodec VideoCodec, AVCPacketType AVCPacketType, uint CompositionTime)
    {
        public const int Size = 5;
    }

    public record struct FlvAudioTagHeader(AudioCodec AudioCodec, int SoundRate, int SoundSize, int SoundType, AACPacketType AACPacketType)
    {
        public const int Size = 2;
    }
}
