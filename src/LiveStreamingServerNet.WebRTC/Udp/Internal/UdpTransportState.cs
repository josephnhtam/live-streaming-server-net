namespace LiveStreamingServerNet.WebRTC.Udp.Internal
{
    public enum UdpTransportState
    {
        New = 1 << 0,
        Started = 1 << 1,
        Closed = 1 << 2
    }

    [Flags]
    public enum UdpTransportStateFlag
    {
        New = 1 << 0,
        Started = 1 << 1,
        Closed = 1 << 2,
    }
}
