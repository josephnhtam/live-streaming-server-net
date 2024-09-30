namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal static class RtmpConstants
    {
        public const uint DefaultChunkSize = 128;

        public const uint ProtocolControlMessageChunkStreamId = 2;
        public const uint ProtocolControlMessageStreamId = 0;

        public const uint UserControlMessageChunkStreamId = 2;
        public const uint UserControlMessageStreamId = 0;

        public const uint OnStatusChunkStreamId = 3;

        public const uint AudioMessageChunkStreamId = 4;
        public const uint VideoMessageChunkStreamId = 5;
        public const uint DataMessageChunkStreamId = 6;
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

    internal enum RtmpPeerBandwidthLimitType : byte
    {
        Hard = 0,
        Soft = 1,
        Dynamic = 2
    }

    internal static class RtmpDataMessageConstants
    {
        public const string SetDataFrame = "@setDataFrame";
        public const string OnMetaData = "onMetaData";
    }
}
