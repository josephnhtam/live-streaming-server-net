namespace LiveStreamingServerNet.Flv.Contracts
{
    /// <summary>
    /// Represents a handle to an FLV client connection, providing methods to manage the connection.
    /// </summary>
    public interface IFlvClientHandle : IFlvClientInfo
    {
        /// <summary>
        /// Stops the FLV client.
        /// </summary>
        void Stop();
    }
}
