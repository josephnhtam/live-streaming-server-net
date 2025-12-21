namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal static class StunMessageType
    {
        public const ushort Request = 0x0001;
        public const ushort Indication = 0x0011;
        public const ushort SuccessResponse = 0x0101;
        public const ushort ErrorResponse = 0x0111;
    }

    internal static class StunMessageMagicCookies
    {
        public const uint Value = 0x2112A442;
    }

    internal enum StunPasswordAlgorithm : ushort
    {
        MD5 = 0x0001,
        SHA256 = 0x0002
    }
}
