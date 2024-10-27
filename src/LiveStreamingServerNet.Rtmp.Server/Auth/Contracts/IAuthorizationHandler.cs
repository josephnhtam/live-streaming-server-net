using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Auth.Contracts
{
    /// <summary>
    /// Handles authorization for RTMP stream publishing and playing requests.
    /// </summary>
    public interface IAuthorizationHandler
    {
        /// <summary>
        /// Gets the execution order of this handler. Lower numbers execute first.
        /// </summary>
        /// <returns>The order value, default is 0</returns>
        int GetOrder() => 0;

        /// <summary>
        /// Authorizes a client's request to publish a stream.
        /// </summary>
        /// <param name="client">Information about the connecting client</param>
        /// <param name="streamPath">The requested stream path</param>
        /// <param name="streamArguments">Additional arguments provided with the publish request</param>
        /// <param name="publishingType">The type of publishing requested (e.g., "live", "record")</param>
        /// <returns>The authorization result indicating if the request is allowed</returns>
        Task<AuthorizationResult> AuthorizePublishingAsync(ISessionInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments, string publishingType);

        /// <summary>
        /// Authorizes a client's request to play a stream.
        /// </summary>
        /// <param name="client">Information about the connecting client</param>
        /// <param name="streamPath">The requested stream path</param>
        /// <param name="streamArguments">Additional arguments provided with the play request</param>
        /// <returns>The authorization result indicating if the request is allowed</returns>
        Task<AuthorizationResult> AuthorizeSubscribingAsync(ISessionInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
