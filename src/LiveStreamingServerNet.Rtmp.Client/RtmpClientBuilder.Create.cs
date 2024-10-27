namespace LiveStreamingServerNet.Rtmp.Client
{
    /// <summary>
    /// Builder class for creating and configuring RTMP clients.
    /// </summary>
    public sealed partial class RtmpClientBuilder
    {
        /// <summary>
        /// Creates a new instance of the RtmpClientBuilder.
        /// </summary>
        /// <returns>A new RtmpClientBuilder instance</returns>
        public static RtmpClientBuilder Create()
        {
            return new RtmpClientBuilder();
        }
    }
}
