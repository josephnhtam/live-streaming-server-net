namespace LiveStreamingServerNet.Rtmp.Client
{
    public enum RtmpClientStatus
    {
        None = 0,
        Connecting = 1,
        HandshakeStarted = 2,
        HandshakeCompleted = 3,
        Connected = 4,
        Stopped = 5
    }
}
