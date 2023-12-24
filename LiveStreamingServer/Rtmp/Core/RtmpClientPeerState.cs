namespace LiveStreamingServer.Rtmp.Core
{
    public enum RtmpClientPeerState
    {
        BeforeHandshake,
        HandshakeC0Received,
        HandshakeC1Received,
        HandshakeC2Received,
        HandshakeDone,
    }
}
