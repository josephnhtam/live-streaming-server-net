using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpCommanderService
    {
        void Command(RtmpCommand command);
        void Command(RtmpCommand command, Func<IRtmpSessionContext, RtmpCommandResult, Task<bool>> callback);
        void Connect(string appName, IDictionary<string, object>? information = null);
        void CreateStream();
    }

    internal record struct RtmpCommand(
        uint chunkStreamId,
        string commandName,
        IReadOnlyDictionary<string, object>? commandObject,
        IReadOnlyList<object?>? parameters = null,
        AmfEncodingType amfEncodingType = AmfEncodingType.Amf0
    );
}
