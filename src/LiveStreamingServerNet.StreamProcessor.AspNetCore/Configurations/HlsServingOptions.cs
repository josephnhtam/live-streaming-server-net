using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;

namespace LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations
{
    /// <summary>
    /// Options for configuring HLS file serving.
    /// </summary>
    public class HlsServingOptions
    {
        private static readonly Action<StaticFileResponseContext> _defaultOnPrepareResponse = _ => { };
        private static readonly Func<HlsFileRequestContext, Task> _defaultOnProcessRequestAsync = _ => Task.CompletedTask;
#if NET8_0_OR_GREATER
        private static readonly Func<StaticFileResponseContext, Task> _defaultOnPrepareResponseAsync = _ => Task.CompletedTask;
#endif

        /// <summary>
        /// Gets or sets the root directory for HLS files. 
        /// Default: {CurrentDirectory}/output
        /// </summary>
        public string Root { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "output");

        /// <summary>
        /// Gets or sets the request path prefix for HLS endpoints.
        /// </summary>
        public PathString RequestPath { get; set; }

        /// <summary>
        /// Gets or sets the HTTPS compression mode.
        /// </summary>
        public HttpsCompressionMode HttpsCompression { get; set; } = HttpsCompressionMode.Compress;

        /// <summary>
        /// Gets or sets the async callback for processing HLS file requests.
        /// </summary>
        public Func<HlsFileRequestContext, Task> OnProcessRequestAsync { get; set; } = _defaultOnProcessRequestAsync;

        /// <summary>
        /// Gets or sets the callback for preparing file responses.
        /// </summary>
        public Action<StaticFileResponseContext> OnPrepareResponse { get; set; } = _defaultOnPrepareResponse;

#if NET8_0_OR_GREATER
        /// <summary>
        /// Gets or sets the async callback for preparing file responses.
        /// </summary>
        public Func<StaticFileResponseContext, Task> OnPrepareResponseAsync { get; set; } = _defaultOnPrepareResponseAsync;
#endif
    }
}
