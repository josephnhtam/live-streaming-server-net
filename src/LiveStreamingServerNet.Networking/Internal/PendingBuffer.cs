using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal record struct PendingBuffer(IRentedBuffer RentedBuffer, Action<bool>? Callback);
}
