namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal enum IceConnectionState
    {
        New = 1 << 0,
        Checking = 1 << 1,
        Connected = 1 << 2,
        Completed = 1 << 3,
        Failed = 1 << 4,
        Closed = 1 << 5
    }

    [Flags]
    internal enum IceConnectionStateFlag
    {
        New = 1 << 0,
        Checking = 1 << 1,
        Connected = 1 << 2,
        Completed = 1 << 3,
        Failed = 1 << 4,
        Closed = 1 << 5
    }
}
