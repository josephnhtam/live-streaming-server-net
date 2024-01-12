using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IHttpFlvClientFactory
    {
        IFlvClient CreateClient(HttpContext context, string streamPath, CancellationToken stoppingToken);
    }
}
