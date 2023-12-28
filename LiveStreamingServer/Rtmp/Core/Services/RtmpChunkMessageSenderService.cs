using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;

namespace LiveStreamingServer.Rtmp.Core.Services
{
    // todo: implement this class. before that, use a large out chunk size.
    public class RtmpChunkMessageSenderService : IRtmpChunkMessageSenderService
    {
        public void Send(IRtmpClientPeerContext peerContext, Action<INetBuffer> writer, Action? callback)
        {
            peerContext.Peer.Send(writer, callback);
        }
    }
}
