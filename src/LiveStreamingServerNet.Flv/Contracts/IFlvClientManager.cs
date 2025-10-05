namespace LiveStreamingServerNet.Flv.Contracts
{
    /// <summary>
    /// Manages FLV client connections and provides access to client handles.
    /// </summary>
    public interface IFlvClientManager
    {
        /// <summary>
        /// Gets a list of FLV client handles for the specified stream path.
        /// </summary>
        /// <param name="streamPath">The path of the stream</param>
        /// <returns>A list of FLV client handles subscribed to the stream</returns>
        IList<IFlvClientHandle> GetFlvClients(string streamPath);
    }
}
