using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpAudioDataProcessorService
    {
        ValueTask<bool> ProcessAudioDataAsync(IRtmpPublishStreamContext publishStreamContext, uint timestamp, IDataBuffer payloadBuffer);
    }
}
