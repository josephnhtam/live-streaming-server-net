using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvClientFactory
    {
        IFlvClient Create(string clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments, IFlvRequest request, IStreamWriter streamWriter, CancellationToken stoppingToken);
    }
}
