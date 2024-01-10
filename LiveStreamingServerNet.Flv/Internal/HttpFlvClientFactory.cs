using LiveStreamingServerNet.Flv.Internal.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class HttpFlvClientFactory : IHttpFlvClientFactory
    {
        private readonly IServiceProvider _services;

        public HttpFlvClientFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IFlvClient CreateClient(HttpContext context, CancellationToken stoppingToken)
        {
            var client = _services.GetRequiredService<IFlvClient>();
            client.Start(new HttpResponseStreamWriter(context.Response), stoppingToken);
            return client;
        }
    }
}
