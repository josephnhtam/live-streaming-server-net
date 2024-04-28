using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvMediaTagManagerService
    {
        ValueTask EnqueueMediaTagAsync(
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
