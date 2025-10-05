namespace LiveStreamingServerNet.Flv.Contracts
{
    /// <summary>
    /// Manages and provides information about FLV streams.
    /// </summary>
    public interface IFlvStreamInfoManager
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
        IList<IFlvStreamInfo> GetStreamInfos();

        /// <summary>
        /// Gets information about a specific stream.
        /// </summary>
        /// <param name="streamPath">The path identifier of the stream</param>
        /// <returns>Stream information object if the stream exists, null otherwise</returns>
        IFlvStreamInfo? GetStreamInfo(string streamPath);
    }
}
