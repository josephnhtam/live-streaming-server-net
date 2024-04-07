namespace LiveStreamingServerNet.Networking.Internal
{
    internal record struct PendingMessage(byte[] RentedBuffer, int BufferSize, Action<bool>? Callback);
}
