using LiveStreamingServerNet.Rtmp;

namespace LiveStreamingServerNet.Standalone.Services.Contracts
{
    /// <summary>
    /// Provides bitrate tracking capabilities for RTMP streams.
    /// </summary>
    public interface IBitrateTrackingService
    {
        /// <summary>
        /// Gets the current video bitrate for a stream in bits per second.
        /// </summary>
        /// <param name="streamPath">The path of the stream</param>
        /// <returns>Video bitrate in bits per second, or 0 if not available</returns>
        int GetCurrentVideoBitrate(string streamPath);

        /// <summary>
        /// Gets the current audio bitrate for a stream in bits per second.
        /// </summary>
        /// <param name="streamPath">The path of the stream</param>
        /// <returns>Audio bitrate in bits per second, or 0 if not available</returns>
        int GetCurrentAudioBitrate(string streamPath);

        /// <summary>
        /// Records data received for a stream.
        /// </summary>
        /// <param name="streamPath">The path of the stream</param>
        /// <param name="mediaType">The type of media</param>
        /// <param name="byteCount">Number of bytes received</param>
        void RecordDataReceived(string streamPath, MediaType mediaType, int byteCount);

        /// <summary>
        /// Cleans up tracking data for a stream when it ends.
        /// </summary>
        /// <param name="streamPath">The path of the stream</param>
        void CleanupStream(string streamPath);
    }
}
