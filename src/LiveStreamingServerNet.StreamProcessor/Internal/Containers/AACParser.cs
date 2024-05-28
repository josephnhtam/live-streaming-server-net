namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
{
    internal static class AACParser
    {
        public static AACSequenceHeader ParseSequenceHeader(ArraySegment<byte> data)
        {
            var objectType = (byte)((data[0] >> 3) & 0x1f);
            var sampleRateIndex = (byte)(((data[0] & 0x07) << 1) | ((data[1] >> 7) & 0x01));
            var channelConfig = (byte)((data[1] >> 3) & 0x0f);

            return new AACSequenceHeader(objectType, sampleRateIndex, channelConfig);
        }
    }
}
