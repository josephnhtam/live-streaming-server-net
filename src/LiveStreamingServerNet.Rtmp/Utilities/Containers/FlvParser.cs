namespace LiveStreamingServerNet.Rtmp.Utilities.Containers
{
    /// <summary>
    /// Provides methods for parsing FLV tag headers.
    /// </summary>
    public static class FlvParser
    {
        /// <summary>
        /// Parses an FLV video tag header from raw byte data.
        /// </summary>
        /// <param name="data">Raw byte data containing the video tag header.</param>
        /// <returns>Parsed FLV video tag header structure.</returns>
        public static FlvVideoTagHeader ParseVideoTagHeader(ReadOnlySpan<byte> data)
        {
            var firstByte = data[0];
            var frameType = (VideoFrameType)(firstByte >> 4);
            var videoCodec = (VideoCodec)(firstByte & 0x0f);
            var packetType = (AVCPacketType)data[1];
            var compositionTime = (uint)(data[2] << 16 | data[3] << 8 | data[4]);

            return new FlvVideoTagHeader(frameType, videoCodec, packetType, compositionTime);
        }

        /// <summary>
        /// Parses an FLV audio tag header from raw byte data.
        /// </summary>
        /// <param name="data">Raw byte data containing the audio tag header.</param>
        /// <returns>Parsed FLV audio tag header structure.</returns>
        public static FlvAudioTagHeader ParseAudioTagHeader(ReadOnlySpan<byte> data)
        {
            var firstByte = data[0];
            var audioCodec = (AudioCodec)(firstByte >> 4);
            var soundRate = firstByte >> 2 & 0x03;
            var soundSize = firstByte >> 1 & 0x01;
            var soundType = firstByte & 0x01;
            var aacPacketType = (AACPacketType)data[1];

            return new FlvAudioTagHeader(audioCodec, soundRate, soundSize, soundType, aacPacketType);
        }
    }
}
