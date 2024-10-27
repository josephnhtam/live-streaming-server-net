using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv
{
    /// <summary>
    /// Record containing context information for an FLV stream request.
    /// </summary>
    /// <param name="HttpContext">The HTTP context of the request.</param>
    /// <param name="StreamPath">The resolved path of the requested stream.</param>
    /// <param name="StreamArguments">Collection of key-value pairs containing stream parameters from the request.</param>
    public record FlvStreamContext(HttpContext HttpContext, string StreamPath, IReadOnlyDictionary<string, string> StreamArguments);
}
