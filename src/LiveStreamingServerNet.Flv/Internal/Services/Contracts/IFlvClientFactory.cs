using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvClientFactory
    {
        IFlvClient Create(string clientId, string streamPath, IStreamWriter streamWriter, CancellationToken stoppingToken);
    }
}
