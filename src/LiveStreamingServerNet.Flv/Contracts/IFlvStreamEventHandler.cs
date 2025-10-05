using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Contracts
{
    /// <summary>
    /// Handles FLV stream playback events.
    /// </summary>
    public interface IFlvServerStreamEventHandler
    {
        /// <summary>
        /// Gets the execution order of this handler. Lower numbers execute first.
        /// </summary>
        /// <returns>The order value, default is 0</returns>
        int GetOrder() => 0;

        /// <summary>
        /// Called when a client begins playing/subscribing to a stream.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="client">The client handle interface</param>
        ValueTask OnFlvStreamSubscribedAsync(IEventContext context, IFlvClientHandle client);

        /// <summary>
        /// Called when a client stops playing/subscribing to a stream.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        ValueTask OnFlvStreamUnsubscribedAsync(IEventContext context, string clientId, string streamPath);
    }
}
