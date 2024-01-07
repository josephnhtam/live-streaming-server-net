using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpProtocolControlMessageSenderService
    {
        void AbortMessage(IRtmpClientContext clientContext, uint streamId);
        void Acknowledgement(IRtmpClientContext clientContext, uint sequenceNumber);
        void SetChunkSize(IRtmpClientContext clientContext, uint chunkSize);
        void SetClientBandwidth(IRtmpClientContext clientContext, uint acknowledgementWindowSize, RtmpClientBandwidthLimitType limitType);
        void WindowAcknowledgementSize(IRtmpClientContext clientContext, uint acknowledgementWindowSize);
    }
}