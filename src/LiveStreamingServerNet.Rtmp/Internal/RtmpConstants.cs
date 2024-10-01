namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal static class RtmpConstants
    {
        public const uint DefaultChunkSize = 128;

        public const uint ControlStreamId = 0;
        public const uint ControlChunkStreamId = 2;

        public const uint ReservedStreamId = 0;
        public const uint ReservedChunkStreamId = 2;
    }

    internal static class RtmpUserControlMessageTypes
    {
        public const ushort StreamBegin = 0;
        public const ushort StreamEOF = 1;
        public const ushort StreamDry = 2;
        public const ushort SetBufferLength = 3;
        public const ushort StreamIsRecorded = 4;
        public const ushort PingRequest = 6;
        public const ushort PingResponse = 7;
    }

    internal static class RtmpDataMessageConstants
    {
        public const string SetDataFrame = "@setDataFrame";
        public const string OnMetaData = "onMetaData";
    }
}
