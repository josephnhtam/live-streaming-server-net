namespace LiveStreamingServerNet.Flv.Contracts
{
    /// <summary>
    /// Provides information about an FLV client connection.
    /// </summary>
    public interface IFlvClientInfo
    {
        /// <summary>
        /// Gets the unique identifier for the FLV client.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// Gets the resolved path of the requested stream.
        /// </summary>
        string StreamPath { get; }

        /// <summary>
        /// Gets the arguments provided when the client requested the stream.
        /// </summary>
        IReadOnlyDictionary<string, string> StreamArguments { get; }

        /// <summary>
        /// Gets request details associated with the FLV client connection.
        /// </summary>
        IFlvRequest Request { get; }
    }
}
