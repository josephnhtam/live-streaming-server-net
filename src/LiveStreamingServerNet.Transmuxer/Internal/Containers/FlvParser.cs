namespace LiveStreamingServerNet.Transmuxer.Internal.Containers
{
    internal static class FlvParser
    {
        public static FlvVideoTagHeader ParseVideoTagHeader(ReadOnlySpan<byte> data)
        {
            var firstByte = data[0];
            var frameType = (VideoFrameType)(firstByte >> 4);
            var codecId = (VideoCodecId)(firstByte & 0x0f);
            var packetType = (AVCPacketType)data[1];
            var compositionTime = (uint)((data[2] << 16) | (data[3] << 8) | data[4]);

            return new FlvVideoTagHeader(frameType, codecId, packetType, compositionTime);
        }

        public static FlvAudioTagHeader ParseAudioTagHeader(ReadOnlySpan<byte> data)
        {
            var firstByte = data[0];
            var soundFormat = (AudioSoundFormat)(firstByte >> 4);
            var soundRate = (firstByte >> 2) & 0x03;
            var soundSize = (firstByte >> 1) & 0x01;
            var soundType = firstByte & 0x01;
            var aacPacketType = (AACPacketType)data[1];

            return new FlvAudioTagHeader(soundFormat, soundRate, soundSize, soundType, aacPacketType);
        }
    }
}
