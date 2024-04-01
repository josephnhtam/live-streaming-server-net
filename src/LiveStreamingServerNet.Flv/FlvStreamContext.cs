using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv
{
    public record FlvStreamContext(HttpContext HttpContext, string StreamPath, IReadOnlyDictionary<string, string> StreamArguments);
}
