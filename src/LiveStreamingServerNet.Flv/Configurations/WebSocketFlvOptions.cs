using LiveStreamingServerNet.Flv.Contracts;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv.Configurations
{
    public class WebSocketFlvOptions
    {
        public IStreamPathResolver? StreamPathResolver { get; set; }
        public WebSocketAcceptContext? WebSocketAcceptContext { get; set; }
        public Func<FlvStreamContext, Task<bool>>? OnPrepareResponse { get; set; }
    }
}
