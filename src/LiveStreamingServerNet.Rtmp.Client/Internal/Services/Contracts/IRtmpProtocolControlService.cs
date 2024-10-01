namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpProtocolControlService
    {
        void AbortMessage(uint streamId);
        void Acknowledgement(uint sequenceNumber);
        void SetChunkSize(uint chunkSize);
        void SetPeerBandwidth(uint peerBandwidth, RtmpBandwidthLimitType limitType);
        void WindowAcknowledgementSize(uint acknowledgementWindowSize);
    }
}