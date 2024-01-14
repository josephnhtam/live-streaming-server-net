using LiveStreamingServerNet.Flv.Internal.Contracts;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv.Internal.HttpClients
{
    internal class HttpResponseStreamWriter : IStreamWriter
    {
        private readonly HttpResponse _response;

        public HttpResponseStreamWriter(HttpResponse response)
        {
            _response = response;
        }

        public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            await _response.BodyWriter.WriteAsync(buffer, cancellationToken);
        }
    }
}
