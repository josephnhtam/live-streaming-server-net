namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal enum RtmpClientSessionState
    {
        HandshakeC0 = 0,
        HandshakeC1 = 1,
        HandshakeC2 = 2,
        HandshakeDone = 3
    }
}
