namespace LiveStreamingServerNet.Rtmp.Client
{
    public record struct RtmpCommand(
        string CommandName,
        IReadOnlyDictionary<string, object>? CommandObject = null,
        IReadOnlyList<object?>? Parameters = null,
        AmfEncodingType AmfEncodingType = AmfEncodingType.Amf0
    );

    public record struct RtmpCommandResponse(double TransactionId, IDictionary<string, object> CommandObject, IList<object>? Parameters);
}
