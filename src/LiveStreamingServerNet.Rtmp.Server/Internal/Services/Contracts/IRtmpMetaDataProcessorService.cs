using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpMetaDataProcessorService
    {
        ValueTask<bool> ProcessMetaDataAsync(IRtmpPublishStreamContext publishStreamContext, uint timestamp, IReadOnlyDictionary<string, object> metaData);
    }
}
