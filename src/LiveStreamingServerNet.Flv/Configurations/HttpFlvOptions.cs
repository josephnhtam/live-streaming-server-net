using LiveStreamingServerNet.Flv.Contracts;

namespace LiveStreamingServerNet.Flv.Configurations
{
    public class HttpFlvOptions
    {
        public IStreamPathResolver? StreamPathResolver { get; set; }
        public Func<FlvStreamContext, Task<bool>>? OnPrepareResponse { get; set; }
    }
}
