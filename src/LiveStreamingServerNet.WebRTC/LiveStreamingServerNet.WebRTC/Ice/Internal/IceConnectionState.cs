namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal enum IceConnectionState
    {
        New,
        Checking,
        Connected,
        Completed,
        Failed,
        Closed
    }
}
