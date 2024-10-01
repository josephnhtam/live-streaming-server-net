using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpMediaDataSenderService
    {
        ValueTask SendMetaDataAsync(IRtmpPublishStreamContext publishStreamContext, IReadOnlyDictionary<string, object> metaData);
        ValueTask SendAudioDataAsync(IRtmpPublishStreamContext publishStreamContext, IRentedBuffer payload, uint timestamp);
        ValueTask SendVideoDataAsync(IRtmpPublishStreamContext publishStreamContext, IRentedBuffer payload, uint timestamp);
    }
}
