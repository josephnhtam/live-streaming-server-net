using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpCommanderService
    {
        void Command(RtmpCommand command);
        void Command(RtmpCommand command, CommandCallbackDelegate callback, Action? cancellationCallback = null);
        void Connect(string appName, IDictionary<string, object>? information = null, ConnectCallbackDelegate? callback = null, Action? cancellationCallback = null);
        void CreateStream(CreateStreamCallbackDelegate? callback = null, Action? cancellationCallback = null);
        void CloseStream(uint streamId);
        void DeleteStream(uint streamId);
        void Play(uint streamId, string streamName, double start, double duration, bool reset);
    }

    internal record struct RtmpCommand(
        uint MessageStreamId,
        uint ChunkStreamId,
        string CommandName,
        IReadOnlyDictionary<string, object>? CommandObject = null,
        IReadOnlyList<object?>? Parameters = null,
        AmfEncodingType AmfEncodingType = AmfEncodingType.Amf0
    );

    internal delegate ValueTask ConnectCallbackDelegate(bool Success, IDictionary<string, object> CommandObject, object? Parameters);
    internal delegate ValueTask CreateStreamCallbackDelegate(bool Success, IRtmpStreamContext? StreamContext);
}
