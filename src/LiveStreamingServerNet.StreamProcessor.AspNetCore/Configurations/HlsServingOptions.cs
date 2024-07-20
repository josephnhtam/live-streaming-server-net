using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;

namespace LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations
{
    public class HlsServingOptions
    {
        private static readonly Action<StaticFileResponseContext> _defaultOnPrepareResponse = _ => { };
        private static readonly Func<HlsFileRequestContext, Task> _defaultOnProcessRequestAsync = _ => Task.CompletedTask;
#if NET8_0_OR_GREATER
        private static readonly Func<StaticFileResponseContext, Task> _defaultOnPrepareResponseAsync = _ => Task.CompletedTask;
#endif

        public string Root { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "output");
        public PathString RequestPath { get; set; }
        public HttpsCompressionMode HttpsCompression { get; set; } = HttpsCompressionMode.Compress;
        public Func<HlsFileRequestContext, Task> OnProcessRequestAsync { get; set; } = _defaultOnProcessRequestAsync;
        public Action<StaticFileResponseContext> OnPrepareResponse { get; set; } = _defaultOnPrepareResponse;

#if NET8_0_OR_GREATER
        public Func<StaticFileResponseContext, Task> OnPrepareResponseAsync { get; set; } = _defaultOnPrepareResponseAsync;
#endif
    }
}
