namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    /// <summary>
    /// Manages information about active RTMP streams.
    /// </summary>
    public interface IRtmpStreamInfoManager
    {
        /// <summary>
        /// Gets a list of all active stream paths.
        /// </summary>
        /// <returns>List of stream path identifiers</returns>
        IList<string> GetStreamPaths();

        /// <summary>
        /// Gets information about all active streams.
        /// </summary>
        /// <returns>List of stream information objects</returns>
        IList<IRtmpStreamInfo> GetStreamInfos();

        /// <summary>
        /// Gets information about a specific stream.
        /// </summary>
        /// <param name="streamPath">The path identifier of the stream</param>
        /// <returns>Stream information object if the stream exists, null otherwise</returns>
        IRtmpStreamInfo? GetStreamInfo(string streamPath);
    }
}
