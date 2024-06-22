using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvMediaTagBroadcasterService
    {
        ValueTask BroadcastMediaTagAsync(
            IFlvStreamContext streamContext,
            IReadOnlyList<IFlvClient> subscribers,
            MediaType mediaType,
            uint timestamp,
            bool isSkippable,
            IRentedBuffer rentedBuffer);

        void RegisterClient(IFlvClient client);
        void UnregisterClient(IFlvClient client);
    }
}
