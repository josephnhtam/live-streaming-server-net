using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttribute(StunAttributeType.ComprehensionRequired.Username)]
    internal record UsernameAttribute(string Username) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.Username;

        public static UsernameAttribute Read(IDataBuffer buffer, int length)
            => new(buffer.ReadUtf8String(length));

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
            => buffer.WriteUtf8String(Username);
    }
}
