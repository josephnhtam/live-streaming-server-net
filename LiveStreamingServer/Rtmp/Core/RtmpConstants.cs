namespace LiveStreamingServer.Rtmp.Core
{
    public static class RtmpConstants
    {
        public const uint DefaultChunkSize = 128;
        public const uint DefaultPeerBandwidth = 500_000;
        public const uint DefaultInAcknowledgementWindowSize = 250_000;
        public const uint DefaultOutAcknowledgementWindowSize = 250_000;

        public const uint ProtocolControlMessageChunkStreamId = 2;
        public const uint ProtocolControlMessageStreamId = 0;
    }

    public enum RtmpPeerBandwidthLimitType : byte
    {
        Hard = 0,
        Soft = 1,
        Dynamic = 2
    }
}
