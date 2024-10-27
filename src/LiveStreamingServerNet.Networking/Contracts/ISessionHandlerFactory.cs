namespace LiveStreamingServerNet.Networking.Contracts
{
    /// <summary>
    /// Factory interface for creating session handlers.
    /// </summary>
    public interface ISessionHandlerFactory
    {
        /// <summary>
        /// Creates new session handler for specified session.
        /// </summary>
        /// <param name="client">Session handle for sending data and controlling session state.</param>
        /// <returns>New session handler instance.</returns>
        ISessionHandler Create(ISessionHandle client);
    }
}
