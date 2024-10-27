namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    /// <summary>
    /// Represents a subscriber to an RTMP downstream.
    /// Implements IDisposable to ensure proper cleanup of resources.
    /// </summary>
    public interface IRtmpDownstreamSubscriber : IDisposable
    {
        /// <summary>
        /// Gets the path of the RTMP stream this subscriber is consuming.
        /// </summary>
        string StreamPath { get; }
    }
}
