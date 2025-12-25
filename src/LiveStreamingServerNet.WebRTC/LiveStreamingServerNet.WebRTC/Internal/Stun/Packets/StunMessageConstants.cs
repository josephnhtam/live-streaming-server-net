namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal enum StunMessageType : ushort
    {
        Request = 0x0001,
        Indication = 0x0011,
        SuccessResponse = 0x0101,
        ErrorResponse = 0x0111
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
