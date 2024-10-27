using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv.Contracts
{
    /// <summary>
    /// Interface for resolving stream paths and arguments from HTTP requests.
    /// </summary>
    public interface IStreamPathResolver
    {
        /// <summary>
        /// Attempts to extract the stream path and arguments from the HTTP request.
        /// </summary>
        /// <param name="context">The HTTP context containing the request information.</param>
        /// <param name="streamPath">The resolved stream path.</param>
        /// <param name="streamArguments">Dictionary of additional stream arguments extracted from the request.</param>
        /// <returns>True if path resolution was successful, false otherwise.</returns>
        bool ResolveStreamPathAndArguments(HttpContext context, out string streamPath, out IDictionary<string, string> streamArguments);
    }
}
