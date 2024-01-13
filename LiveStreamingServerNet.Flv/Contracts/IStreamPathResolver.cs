using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv.Contracts
{
    public interface IStreamPathResolver
    {
        bool ResolveStreamPathAndArguments(HttpContext context, out string streamPath, out IDictionary<string, string> streamArguments);
    }
}
