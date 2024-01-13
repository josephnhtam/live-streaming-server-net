using LiveStreamingServerNet.Flv.Internal.Contracts;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv.Internal.HttpClients.Contracts
{
    internal interface IHttpFlvClientFactory
    {
        IFlvClient CreateClient(HttpContext context, string streamPath, CancellationToken stoppingToken);
    }
}
