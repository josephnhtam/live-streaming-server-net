namespace LiveStreamingServerNet.Rtmp.Utilities.Containers
{
    /// <summary>
    /// Represents the header structure of an FLV video tag.
    /// </summary>
    /// <param name="FrameType">Type of video frame (keyframe, inter frame, etc.).</param>
    /// <param name="VideoCodec">Video codec identifier.</param>
    /// <param name="AVCPacketType">AVC/H.264 packet type.</param>
    /// <param name="CompositionTime">Composition time offset in milliseconds.</param>
    public record struct FlvVideoTagHeader(VideoFrameType FrameType, VideoCodec VideoCodec, AVCPacketType AVCPacketType, uint CompositionTime)
    {
        /// <summary>
        /// Size of the video tag header in bytes.
        /// </summary>
        public const int Size = 5;
    }

    /// <summary>
    /// Represents the header structure of an FLV audio tag.
    /// </summary>
    /// <param name="AudioCodec">Audio codec identifier.</param>
    /// <param name="SoundRate">Audio sampling rate (e.g., 44100Hz).</param>
    /// <param name="SoundSize">Audio sample size in bits.</param>
    /// <param name="SoundType">Audio channel type (mono/stereo).</param>
    /// <param name="AACPacketType">AAC packet type for AAC audio.</param>
    public record struct FlvAudioTagHeader(AudioCodec AudioCodec, int SoundRate, int SoundSize, int SoundType, AACPacketType AACPacketType)
    {
        /// <summary>
        /// Size of the audio tag header in bytes.
        /// </summary>
        public const int Size = 2;
    }
}
