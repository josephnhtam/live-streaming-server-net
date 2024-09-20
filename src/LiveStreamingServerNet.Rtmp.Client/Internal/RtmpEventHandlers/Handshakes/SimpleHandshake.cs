using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.Handshakes
{
    internal class SimpleHandshake
    {
        private const byte _clientType = 3;

        public bool ValidateS1(IRtmpSessionContext context, IDataBuffer s1Buffer)
        {
            return true;
        }

        public void WriteC0(IDataBuffer outgoingBuffer)
        {
            outgoingBuffer.Write(_clientType);
        }

        public void WriteC1(IRtmpSessionContext context, IDataBuffer outgoingBuffer)
        {
            var c1Time = HandshakeUtilities.GetTime();
            context.Items["C1Time"] = c1Time;

            outgoingBuffer.Write(c1Time);
            outgoingBuffer.Write(HandshakeUtilities.FourZeroBytes.Span);
            outgoingBuffer.WriteRandomBytes(1536 - 8);
        }

        public void WriteC2(IRtmpSessionContext context, IDataBuffer s1Buffer, IDataBuffer outgoingBuffer)
        {
            var c1Time = context.Items.GetValueOrDefault("C1Time", 0);

            var s1Time = s1Buffer.ReadInt32();
            s1Buffer.Advance(4);
            var randomEcho = s1Buffer.ReadBytes(1528);

            outgoingBuffer.Write(s1Time);
            outgoingBuffer.Write(c1Time);
            outgoingBuffer.Write(randomEcho);
        }
    }
}