using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpProtocolControlMessageSenderService
    {
        void AbortMessage(IRtmpClientSessionContext clientContext, uint streamId);
        void Acknowledgement(IRtmpClientSessionContext clientContext, uint sequenceNumber);
        void SetChunkSize(IRtmpClientSessionContext clientContext, uint chunkSize);
        void SetClientBandwidth(IRtmpClientSessionContext clientContext, uint clientBandwidth, RtmpClientBandwidthLimitType limitType);
        void WindowAcknowledgementSize(IRtmpClientSessionContext clientContext, uint windowAcknowledgementSize);
    }
}