namespace LiveStreamingServerNet.Rtmp.Client
{
    /// <summary>
    /// Represents an RTMP command request to be sent to the server.
    /// </summary>
    /// <param name="CommandName">The name of the RTMP command (e.g., "connect", "createStream", "publish")</param>
    /// <param name="CommandObject">Command information object</param>
    /// <param name="Parameters">Optional command-specific parameters</param>
    /// <param name="AmfEncodingType">The AMF encoding type to use for serializing the command (default: AMF0)</param>
    public record struct RtmpCommand(
        string CommandName,
        IReadOnlyDictionary<string, object>? CommandObject = null,
        IReadOnlyList<object?>? Parameters = null,
        AmfEncodingType AmfEncodingType = AmfEncodingType.Amf0
    );

    /// <summary>
    /// Represents a response received from the server for an RTMP command.
    /// </summary>
    /// <param name="TransactionId">The transaction ID that matches the original command request</param>
    /// <param name="CommandObject">Command information object</param>
    /// <param name="Parameters">Optional command-specific response parameters</param>
    public record struct RtmpCommandResponse(double TransactionId, IDictionary<string, object> CommandObject, IList<object>? Parameters);
}
