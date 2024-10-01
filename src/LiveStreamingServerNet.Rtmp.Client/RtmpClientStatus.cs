namespace LiveStreamingServerNet.Rtmp.Client
{
    public enum RtmpClientStatus
    {
        None = 0,
        Handshaking = 1,
        Connecting = 2,
        Connected = 3,
        Stopped = 4
    }
}
