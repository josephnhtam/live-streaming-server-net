using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations
{
    public class HlsServingOptions
    {
        public string Root { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "output");
        public PathString RequestPath { get; set; }
        public HttpsCompressionMode HttpsCompression { get; set; } = HttpsCompressionMode.Compress;
    }
}
