using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttribute(StunAttributeType.ComprehensionRequired.XorMappedAddress)]
    internal record XorMappedAddressAttribute(IPAddress Address, ushort Port) : IStunAttribute
    {
        private const ushort XorPortMask = 0x2112;
        private static readonly byte[] XorAddressMaskV4 = new byte[] { 0x21, 0x12, 0xA4, 0x42 };

        public ushort Type => StunAttributeType.ComprehensionRequired.XorMappedAddress;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
        {
            buffer.Write((byte)0x00);

            switch (Address.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    {
                        buffer.Write((byte)0x01);
                        buffer.WriteUInt16BigEndian((ushort)(Port ^ XorPortMask));

                        var mask = XorAddressMaskV4;
                        Span<byte> addressBytes = stackalloc byte[4];
                        Address.TryWriteBytes(addressBytes, out _);

                        for (var i = 0; i < 4; i++)
                            buffer.Write((byte)(addressBytes[i] ^ mask[i]));

                        break;
                    }

                case AddressFamily.InterNetworkV6:
                    {
                        buffer.Write((byte)0x02);
                        buffer.WriteUInt16BigEndian((ushort)(Port ^ XorPortMask));

                        Span<byte> mask = stackalloc byte[16];
                        XorAddressMaskV4.AsSpan().CopyTo(mask.Slice(0, 4));
                        request.TransactionId.Span.CopyTo(mask.Slice(4));

                        Span<byte> addressBytes = stackalloc byte[16];
                        Address.TryWriteBytes(addressBytes, out _);

                        for (var i = 0; i < 16; i++)
                            buffer.Write((byte)(addressBytes[i] ^ mask[i]));

                        break;
                    }

                default:
                    throw new ArgumentException("Only IPv4 and IPv6 are allowed.");
            }
        }
    }
}
