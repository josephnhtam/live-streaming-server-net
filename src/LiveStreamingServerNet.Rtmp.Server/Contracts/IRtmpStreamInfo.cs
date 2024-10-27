using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    /// <summary>
    /// Provides information about an active RTMP stream.
    /// </summary>
    public interface IRtmpStreamInfo
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
        /// Gets the time when the stream started publishing.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Gets the session control interface for the publishing client.
        /// </summary>
        ISessionControl Publisher { get; }

        /// <summary>
        /// Gets a list of session control interfaces for all current subscribers/viewers.
        /// </summary>
        IReadOnlyList<ISessionControl> Subscribers { get; }
    }
}
