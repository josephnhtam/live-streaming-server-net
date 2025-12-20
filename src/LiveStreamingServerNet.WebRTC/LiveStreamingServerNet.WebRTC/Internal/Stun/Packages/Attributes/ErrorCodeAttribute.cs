using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes
{
    internal record ErrorCodeAttribute(ushort Code, string Reason) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.ErrorCode;

        public void Write(BindingRequest request, IDataBuffer buffer)
        {
            var classValue = (byte)(Code / 100);
            var numberValue = (byte)(Code % 100);

            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);

            buffer.Write(classValue);
            buffer.Write(numberValue);
            
            buffer.WriteUtf8String(Reason);
        }
    }
}
