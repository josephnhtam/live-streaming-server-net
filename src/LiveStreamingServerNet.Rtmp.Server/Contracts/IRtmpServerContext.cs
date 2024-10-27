namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    /// <summary>
    /// Provides access to the RTMP server's runtime context.
    /// </summary>
    public interface IRtmpServerContext
    {
        /// <summary>
        /// Gets the auth code for bypassing stream access validation.
        /// Typically, used by internal services (e.g., FFmpeg transcoding) to access streams directly without requiring standard authorization.
        /// </summary>
        string AuthCode { get; }
    }
}
