namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal record struct RtmpCommandMessage(
        uint MessageStreamId,
        uint ChunkStreamId,
        string CommandName,
        IReadOnlyDictionary<string, object>? CommandObject = null,
        IReadOnlyList<object?>? Parameters = null,
        AmfEncodingType AmfEncodingType = AmfEncodingType.Amf0
    );
}
