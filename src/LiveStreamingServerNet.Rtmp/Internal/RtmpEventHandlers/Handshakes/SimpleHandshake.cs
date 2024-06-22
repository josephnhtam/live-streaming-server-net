using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Handshakes
{
    internal class SimpleHandshake
    {
        private const byte _clientType = 3;

        private readonly IDataBuffer _incomingBuffer;

        public SimpleHandshake(IDataBuffer incomingBuffer)
        {
            _incomingBuffer = incomingBuffer;
        }

        public bool ValidateC1()
        {
            return true;
        }

        public void WriteS0S1S2(IDataBuffer outgoingBuffer)
        {
            WriteS0(outgoingBuffer);
            WriteS1(outgoingBuffer);
            WriteS2(outgoingBuffer);
        }

        public void WriteS0(IDataBuffer outgoingBuffer)
        {
            outgoingBuffer.Write(_clientType);
        }

        public void WriteS1(IDataBuffer outgoingBuffer)
        {
            outgoingBuffer.Write(HandshakeUtilities.GetTime());
            outgoingBuffer.Write(RtmpConstants.ServerVersion);
            outgoingBuffer.WriteRandomBytes(1536 - 8);
        }

        public void WriteS2(IDataBuffer outgoingBuffer)
        {
            _incomingBuffer.CopyAllTo(outgoingBuffer);
        }
    }
}
