namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal static class HandshakeUtilities
    {
        public static int GetTime()
        {
            return (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        }

        public static ReadOnlyMemory<byte> FourZeroBytes = new byte[4];
    }
}
