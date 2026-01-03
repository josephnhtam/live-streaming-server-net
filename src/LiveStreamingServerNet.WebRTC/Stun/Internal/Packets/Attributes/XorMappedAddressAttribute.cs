using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionRequired.XorMappedAddress)]
    internal record XorMappedAddressAttribute(IPEndPoint EndPoint) : IStunAttribute
    {
        private const ushort XorPortMask = 0x2112;
        private static readonly byte[] XorAddressMaskV4 = new byte[] { 0x21, 0x12, 0xA4, 0x42 };

        public ushort Type => StunAttributeTypes.ComprehensionRequired.XorMappedAddress;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
        {
            var address = EndPoint.Address;
            var port = (ushort)EndPoint.Port;

            buffer.Write((byte)0x00);

            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    {
                        buffer.Write((byte)0x01);
                        buffer.WriteUInt16BigEndian((ushort)(port ^ XorPortMask));

                        var mask = XorAddressMaskV4;
                        Span<byte> addressBytes = stackalloc byte[4];
                        address.TryWriteBytes(addressBytes, out _);

                        for (var i = 0; i < 4; i++)
                            buffer.Write((byte)(addressBytes[i] ^ mask[i]));

                        break;
                    }

                case AddressFamily.InterNetworkV6:
                    {
                        buffer.Write((byte)0x02);
                        buffer.WriteUInt16BigEndian((ushort)(port ^ XorPortMask));

                        Span<byte> mask = stackalloc byte[16];
                        ConstructIPV6Mask(mask, transactionId);

                        Span<byte> addressBytes = stackalloc byte[16];
                        address.TryWriteBytes(addressBytes, out _);

                        for (var i = 0; i < 16; i++)
                            buffer.Write((byte)(addressBytes[i] ^ mask[i]));

                        break;
                    }

                default:
                    throw new ArgumentException("Only IPv4 and IPv6 are allowed.");
            }
        }

        public static XorMappedAddressAttribute ReadValue(TransactionId transactionId, IDataBufferReader buffer, ushort length)
        {
            buffer.ReadByte();

            var familyByte = buffer.ReadByte();
            switch (familyByte)
            {
                case 0x01:
                    {
                        var port = (ushort)(buffer.ReadUInt16BigEndian() ^ XorPortMask);

                        Span<byte> addressBytes = stackalloc byte[4];
                        buffer.ReadBytes(addressBytes);

                        var mask = XorAddressMaskV4;
                        for (int i = 0; i < 4; i++)
                            addressBytes[i] ^= mask[i];

                        var address = new IPAddress(addressBytes);
                        return new XorMappedAddressAttribute(new IPEndPoint(address, port));
                    }

                case 0x02:
                    {
                        var port = (ushort)(buffer.ReadUInt16BigEndian() ^ XorPortMask);

                        Span<byte> addressBytes = stackalloc byte[16];
                        buffer.ReadBytes(addressBytes);

                        Span<byte> mask = stackalloc byte[16];
                        ConstructIPV6Mask(mask, transactionId);

                        for (int i = 0; i < 4; i++)
                            addressBytes[i] ^= mask[i];

                        var address = new IPAddress(addressBytes);
                        return new XorMappedAddressAttribute(new IPEndPoint(address, port));
                    }

                default:
                    throw new ArgumentException("Only IPv4 and IPv6 are allowed.");
            }
        }

        private static void ConstructIPV6Mask(Span<byte> mask, TransactionId transactionId)
        {
            Debug.Assert(mask.Length == 16);

            XorAddressMaskV4.AsSpan().CopyTo(mask.Slice(0, 4));
            transactionId.Span.CopyTo(mask.Slice(4));
        }
    }
}
