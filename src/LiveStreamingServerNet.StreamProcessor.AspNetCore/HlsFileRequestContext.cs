using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.StreamProcessor.AspNetCore
{
    /// <summary>
    /// Represents the context for an HLS file request.
    /// </summary>
    public class HlsFileRequestContext
    {
        /// <summary>
        /// Gets the HTTP context of the request.
        /// </summary>
        public HttpContext Context { get; }

        /// <summary>
        /// Gets the path of the stream being requested.
        /// </summary>
        public string StreamPath { get; }

        /// <summary>
        /// Gets the physical file path of the requested HLS file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Creates a new HLS file request context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="streamPath">The stream path.</param>
        /// <param name="filePath">The physical file path.</param>
        public HlsFileRequestContext(HttpContext context, string streamPath, string filePath)
        {
            Context = context;
            StreamPath = streamPath;
            FilePath = filePath;
        }
    }
}
