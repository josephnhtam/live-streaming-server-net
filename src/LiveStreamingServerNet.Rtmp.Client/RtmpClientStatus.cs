namespace LiveStreamingServerNet.Rtmp.Client
{
    /// <summary>
    /// Represents the current status of an RTMP client connection.
    /// </summary>
    public enum RtmpClientStatus
    {
        None = 0,
        Handshaking = 1,
        Connecting = 2,
        Connected = 3,
        Stopped = 4
    }
}
