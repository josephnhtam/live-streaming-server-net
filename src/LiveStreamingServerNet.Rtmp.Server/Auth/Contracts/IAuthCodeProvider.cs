namespace LiveStreamingServerNet.Rtmp.Server.Auth.Contracts
{
    /// <summary>
    /// Provides auth codes that grant unrestricted access to RTMP streams.
    /// </summary>
    public interface IAuthCodeProvider
    {
        /// <summary>
        /// Gets the auth code for bypassing stream access validation.
        /// Typically, used by internal services (e.g., FFmpeg transcoding) to access streams directly without requiring standard authorization.
        /// </summary>
        /// <returns>An auth code for unrestricted access</returns>
        string GetAuthCode();
    }
}
