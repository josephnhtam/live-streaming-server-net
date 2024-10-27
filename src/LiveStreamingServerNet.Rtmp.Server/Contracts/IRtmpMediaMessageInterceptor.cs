using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    /// <summary>
    /// Intercepts and handles RTMP media messages.
    /// </summary>
    public interface IRtmpMediaMessageInterceptor
    {
        /// <summary>
        /// Determines whether to intercept the media message for a specific client and stream.
        /// </summary>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        /// <param name="mediaType">The type of media (audio/video)</param>
        /// <param name="timestamp">The timestamp of the media in milliseconds</param>
        /// <param name="isSkippable">Whether the message can be safely skipped without affecting playback</param>
        /// <returns>True to intercept the message, false to skip interception</returns>
        bool FilterMediaMessage(uint clientId, string streamPath, MediaType mediaType, uint timestamp, bool isSkippable) => true;

        /// <summary>
        /// Called when a media message is received.
        /// </summary>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        /// <param name="mediaType">The type of media (audio/video)</param>
        /// <param name="rentedBuffer">The buffer containing media data</param>
        /// <param name="timestamp">The timestamp of the media in milliseconds</param>
        /// <param name="isSkippable">Whether the message can be safely skipped without affecting playback</param>
        ValueTask OnReceiveMediaMessageAsync(uint clientId, string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable);
    }
}
