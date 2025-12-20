namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages
{
    public static class StunMessageType
    {
        public const ushort Request = 0x0001;
        public const ushort Indication = 0x0011;
        public const ushort SuccessResponse = 0x0101;
        public const ushort ErrorResponse = 0x0111;
    }

    public static class StunMessageMagicCookies
    {
        public const uint Value = 0x2112A442;
    }
}
