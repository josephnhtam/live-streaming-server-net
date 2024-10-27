using LiveStreamingServerNet.Contracts;

namespace LiveStreamingServerNet
{
    /// <summary>
    /// Builder class for configuring and creating LiveStreamingServer instances.
    /// </summary>
    public sealed partial class LiveStreamingServerBuilder
    {
        /// <summary>
        /// Creates new instance of LiveStreamingServerBuilder.
        /// </summary>
        /// <returns>Builder interface for fluent configuration.</returns>
        public static ILiveStreamingServerBuilder Create()
        {
            return new LiveStreamingServerBuilder();
        }
    }
}
