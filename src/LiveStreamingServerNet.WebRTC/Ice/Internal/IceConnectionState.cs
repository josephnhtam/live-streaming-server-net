namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal enum IceConnectionState
    {
        New = 1 << 0,
        Checking = 1 << 1,
        Connected = 1 << 2,
        Completed = 1 << 3,
        Disconnected = 1 << 4,
        Failed = 1 << 5,
        Closed = 1 << 6
    }

    [Flags]
    internal enum IceConnectionStateFlag
    {
        New = 1 << 0,
        Checking = 1 << 1,
        Connected = 1 << 2,
        Completed = 1 << 3,
        Disconnected = 1 << 4,
        Failed = 1 << 5,
        Closed = 1 << 6
    }
}
