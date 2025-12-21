using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionRequired.ErrorCode)]
    internal record ErrorCodeAttribute(ushort Code, string Reason) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.ErrorCode;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
        {
            var classValue = (byte)(Code / 100);
            var numberValue = (byte)(Code % 100);

            buffer.WriteUInt16BigEndian(0x00);

            buffer.Write(classValue);
            buffer.Write(numberValue);

            buffer.WriteUtf8String(Reason);
        }

        public static ErrorCodeAttribute ReadValue(IDataBuffer buffer, ushort length)
        {
            buffer.ReadUInt16BigEndian();

            var classValue = buffer.ReadByte();
            var numberValue = buffer.ReadByte();
            var reason = buffer.ReadUtf8String(length - 4);

            return new ErrorCodeAttribute((ushort)(classValue * 100 + numberValue), reason);
        }
    }
}
