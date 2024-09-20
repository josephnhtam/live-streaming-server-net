namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal enum RtmpSessionState
    {
        HandshakeS0 = 0,
        HandshakeS1 = 1,
        HandshakeS2 = 2,
        HandshakeDone = 3
    }
}