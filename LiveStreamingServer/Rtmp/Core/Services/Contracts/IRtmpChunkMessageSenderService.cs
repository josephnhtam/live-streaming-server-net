using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core.Services.Contracts
{
    public interface IRtmpChunkMessageSenderService
    {
        void Send(IRtmpClientPeerContext peerContext, Action<INetBuffer> writer, Action? callback = null);
    }
}