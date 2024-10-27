using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    /// <summary>
    /// Handles RTMP server connection lifecycle events.
    /// </summary>
    public interface IRtmpServerConnectionEventHandler
    {
        /// <summary>
        /// Gets the execution order of this handler. Lower numbers execute first.
        /// </summary>
        /// <returns>The order value, default is 0</returns>
        int GetOrder() => 0;

        /// <summary>
        /// Called when a new RTMP client is created but before handshake begins.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="client">The client session control interface</param>
        ValueTask OnRtmpClientCreatedAsync(IEventContext context, ISessionControl client);

        /// <summary>
        /// Called when an RTMP client is about to be disposed.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="clientId">The ID of the client</param>
        ValueTask OnRtmpClientDisposingAsync(IEventContext context, uint clientId);

        /// <summary>
        /// Called after an RTMP client has been disposed.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="clientId">The ID of the client</param>
        ValueTask OnRtmpClientDisposedAsync(IEventContext context, uint clientId);

        /// <summary>
        /// Called when RTMP handshake is successfully completed.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="clientId">The ID of the client</param>
        ValueTask OnRtmpClientHandshakeCompleteAsync(IEventContext context, uint clientId);

        /// <summary>
        /// Called when an RTMP client successfully connects with connect command.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="commandObject">The command object containing connection parameters</param>
        /// <param name="arguments">Optional additional arguments</param>
        ValueTask OnRtmpClientConnectedAsync(IEventContext context, uint clientId, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    /// <summary>
    /// Handles RTMP stream publishing and playback events.
    /// </summary>
    public interface IRtmpServerStreamEventHandler
    {
        /// <summary>
        /// Gets the execution order of this handler. Lower numbers execute first.
        /// </summary>
        /// <returns>The order value, default is 0</returns>
        int GetOrder() => 0;

        /// <summary>
        /// Called when a client begins publishing a stream.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        /// <param name="streamArguments">Arguments included in the publish request</param>
        ValueTask OnRtmpStreamPublishedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);

        /// <summary>
        /// Called when a client stops publishing a stream.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath);

        /// <summary>
        /// Called when a client begins playing/subscribing to a stream.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        /// <param name="streamArguments">Arguments included in the play request</param>
        ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);

        /// <summary>
        /// Called when a client stops playing/subscribing to a stream.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, uint clientId, string streamPath);

        /// <summary>
        /// Called when stream metadata is received from a publisher.
        /// </summary>
        /// <param name="context">The event context</param>
        /// <param name="clientId">The ID of the client</param>
        /// <param name="streamPath">The path of the stream</param>
        /// <param name="metaData">The metadata key-value pairs</param>
        ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData);
    }
}
