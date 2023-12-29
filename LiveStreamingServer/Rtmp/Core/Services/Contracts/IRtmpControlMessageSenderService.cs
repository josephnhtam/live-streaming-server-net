using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core.Services.Contracts
{
    public interface IRtmpControlMessageSenderService
    {
        void AbortMessage(IRtmpClientPeerContext peerContext, uint streamId);
        void Acknowledgement(IRtmpClientPeerContext peerContext, uint sequenceNumber);
        void SetChunkSize(IRtmpClientPeerContext peerContext, uint chunkSize);
        void SetPeerBandwidth(IRtmpClientPeerContext peerContext, uint acknowledgementWindowSize, RtmpPeerBandwidthLimitType limitType);
        void WindowAcknowledgementSize(IRtmpClientPeerContext peerContext, uint acknowledgementWindowSize);
    }
}