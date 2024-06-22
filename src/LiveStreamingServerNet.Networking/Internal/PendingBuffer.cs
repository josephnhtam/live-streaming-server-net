using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal record struct PendingBuffer(IRentedBuffer RentedBuffer, Action<bool>? Callback);
}
