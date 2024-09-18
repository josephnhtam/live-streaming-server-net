using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Handshakes
{
    internal class SimpleHandshake
    {
        private const byte _clientType = 3;
        private readonly IDataBuffer _incomingBuffer;

        private int _s1Time;

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
            _s1Time = HandshakeUtilities.GetTime();

            outgoingBuffer.Write(_s1Time);
            outgoingBuffer.Write(HandshakeUtilities.FourZeroBytes.Span);
            outgoingBuffer.WriteRandomBytes(1536 - 8);
        }

        public void WriteS2(IDataBuffer outgoingBuffer)
        {
            var c1Time = _incomingBuffer.ReadInt32();
            _incomingBuffer.Advance(4);
            var randomEcho = _incomingBuffer.ReadBytes(1528);

            outgoingBuffer.Write(c1Time);
            outgoingBuffer.Write(_s1Time);
            outgoingBuffer.Write(randomEcho);
        }
    }
}
