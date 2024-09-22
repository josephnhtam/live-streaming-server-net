using LiveStreamingServerNet.Rtmp.Internal;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpProtocolControlService
    {
        void AbortMessage(uint streamId);
        void Acknowledgement(uint sequenceNumber);
        void SetChunkSize(uint chunkSize);
        void SetPeerBandwidth(uint acknowledgementWindowSize, RtmpPeerBandwidthLimitType limitType);
        void WindowAcknowledgementSize(uint acknowledgementWindowSize);
    }
}