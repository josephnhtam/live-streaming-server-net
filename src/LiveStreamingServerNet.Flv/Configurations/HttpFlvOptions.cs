using LiveStreamingServerNet.Flv.Contracts;

namespace LiveStreamingServerNet.Flv.Configurations
{
    /// <summary>
    /// Configuration options for HTTP FLV streaming.
    /// </summary>
    public class HttpFlvOptions
    {
        /// <summary>
        /// Resolves stream paths for HTTP FLV requests.
        /// If not set, default path resolution will be used.
        /// </summary>
        public IStreamPathResolver? StreamPathResolver { get; set; }

        /// <summary>
        /// Callback function executed before sending the FLV response.
        /// Returns true to continue with the response, false to abort.
        /// Allows for custom validation and response preparation.
        /// </summary>
        public Func<FlvStreamContext, Task<bool>>? OnPrepareResponse { get; set; }
    }
}
