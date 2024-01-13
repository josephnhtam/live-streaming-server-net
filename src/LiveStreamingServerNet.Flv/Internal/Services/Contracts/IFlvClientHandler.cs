using LiveStreamingServerNet.Flv.Internal.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvClientHandler
    {
        Task RunClientAsync(IFlvClient client);
    }
}
