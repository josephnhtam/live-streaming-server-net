namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal enum StunClass : ushort
    {
        Request = 0,
        Indication = 1,
        SuccessResponse = 2,
        ErrorResponse = 3
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
