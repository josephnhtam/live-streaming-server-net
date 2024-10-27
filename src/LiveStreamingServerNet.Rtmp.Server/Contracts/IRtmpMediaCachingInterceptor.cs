using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    /// <summary>
    /// Intercepts and handles RTMP media caching operations.
    /// </summary>
    public interface IRtmpMediaCachingInterceptor
    {
        /// <summary>
        /// Determines whether to intercept the caching event for a specific client and stream.
        /// </summary>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        /// <param name="mediaType">The type of media (audio/video)</param>
        /// <returns>True to intercept the event, false to skip interception</returns>
        bool FilterCache(uint clientId, string streamPath, MediaType mediaType) => true;

        /// <summary>
        /// Called when a sequence header is being cached.
        /// </summary>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        /// <param name="mediaType">The type of media (audio/video)</param>
        /// <param name="sequenceHeader">The codec configuration data</param>
        ValueTask OnCacheSequenceHeaderAsync(uint clientId, string streamPath, MediaType mediaType, byte[] sequenceHeader);

        /// <summary>
        /// Called when a video frame or audio sample is being cached.
        /// </summary>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        /// <param name="mediaType">The type of media (audio/video)</param>
        /// <param name="rentedBuffer">The buffer containing media data</param>
        /// <param name="timestamp">The timestamp of the media in milliseconds</param>
        ValueTask OnCachePictureAsync(uint clientId, string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);

        /// <summary>
        /// Called when the GOP cache for a stream is being cleared.
        /// </summary>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        ValueTask OnClearGroupOfPicturesCacheAsync(uint clientId, string streamPath);
    }
}
