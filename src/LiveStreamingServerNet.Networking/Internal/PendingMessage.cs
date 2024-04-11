using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal record struct PendingMessage(IRentedBuffer RentedBuffer, Action<bool>? Callback);
}
