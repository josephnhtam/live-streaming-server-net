using LiveStreamingServer.Newtorking.Contracts;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler.Handshakes
{
    public class SimpleHandshake
    {
        private const byte ClientType = 3;

        private readonly INetBuffer _incomingBuffer;

        public SimpleHandshake(INetBuffer incomingBuffer)
        {
            _incomingBuffer = incomingBuffer;
        }

        public bool ValidateC1()
        {
            return true;
        }

        public void WriteS0S1S2(INetBuffer outgoingBuffer)
        {
            WriteS0(outgoingBuffer);
            WriteS1(outgoingBuffer);
            WriteS2(outgoingBuffer);
        }

        public void WriteS0(INetBuffer outgoingBuffer)
        {
            outgoingBuffer.Write(ClientType);
        }

        public void WriteS1(INetBuffer outgoingBuffer)
        {
            outgoingBuffer.Write(HandshakeUtilities.GetTime());
            outgoingBuffer.Write(_incomingBuffer.UnderlyingStream.GetBuffer(), 4, 4);
            outgoingBuffer.WriteRandomBytes(1536 - 8);
        }

        public void WriteS2(INetBuffer outgoingBuffer)
        {
            outgoingBuffer.Write(_incomingBuffer.UnderlyingStream.GetBuffer(), 0, 1536);
        }
    }
}
