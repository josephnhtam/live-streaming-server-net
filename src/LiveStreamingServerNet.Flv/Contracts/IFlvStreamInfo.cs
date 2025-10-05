namespace LiveStreamingServerNet.Flv.Contracts
{
    /// <summary>
    /// Provides information about an active FLV stream.
    /// </summary>
    public interface IFlvStreamInfo
    {
        /// <summary>
        /// Gets the path identifier of the stream.
        /// </summary>
        string StreamPath { get; }

        /// <summary>
        /// Gets the arguments provided when the stream was published.
        /// </summary>
        IReadOnlyDictionary<string, string> StreamArguments { get; }

        /// <summary>
        /// Gets the stream metadata sent by the publisher, if any.
        /// Contains information like video dimensions, framerate, etc.
        /// </summary>
        IReadOnlyDictionary<string, object>? MetaData { get; }
        
        /// <summary>
        /// Gets a list of client handle interfaces for all current subscribers/viewers.
        /// </summary>
        IReadOnlyList<IFlvClientHandle> Subscribers { get; }
    }
}
