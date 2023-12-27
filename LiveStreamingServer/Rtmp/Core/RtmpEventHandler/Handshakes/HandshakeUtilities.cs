namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Handshakes
{
    public static class HandshakeUtilities
    {
        public static int GetTime()
        {
            return (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        }
    }
}
