using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.StreamProcessor.AspNetCore
{
    public class HlsFileRequestContext
    {
        public HttpContext Context { get; }
        public string StreamPath { get; }
        public string FilePath { get; }

        public HlsFileRequestContext(HttpContext context, string streamPath, string filePath)
        {
            Context = context;
            StreamPath = streamPath;
            FilePath = filePath;
        }
    }
}
