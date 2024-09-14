namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal enum RtmpClientState
    {
        HandshakeC0 = 0,
        HandshakeC1 = 1,
        HandshakeC2 = 2,
        HandshakeDone = 3
    }
}
