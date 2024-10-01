using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpProtocolControlService
    {
        void AbortMessage(IRtmpClientSessionContext clientContext, uint streamId);
        void Acknowledgement(IRtmpClientSessionContext clientContext, uint sequenceNumber);
        void SetChunkSize(IRtmpClientSessionContext clientContext, uint chunkSize);
        void SetPeerBandwidth(IRtmpClientSessionContext clientContext, uint peerBandwidth, RtmpBandwidthLimitType limitType);
        void WindowAcknowledgementSize(IRtmpClientSessionContext clientContext, uint windowAcknowledgementSize);
    }
}