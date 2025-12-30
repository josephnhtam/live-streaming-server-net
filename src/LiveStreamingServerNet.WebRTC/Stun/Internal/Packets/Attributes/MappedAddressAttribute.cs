using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionRequired.MappedAddress)]
    internal record MappedAddressAttribute(IPEndPoint EndPoint) : IStunAttribute
    {
        public ushort Type => StunAttributeTypes.ComprehensionRequired.MappedAddress;

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
                        buffer.WriteUInt16BigEndian(port);

                        Span<byte> addressBytes = stackalloc byte[4];
                        address.TryWriteBytes(addressBytes, out _);
                        buffer.Write(addressBytes);
                        break;
                    }

                case AddressFamily.InterNetworkV6:
                    {
                        buffer.Write((byte)0x02);
                        buffer.WriteUInt16BigEndian(port);

                        Span<byte> addressBytes = stackalloc byte[16];
                        address.TryWriteBytes(addressBytes, out _);
                        buffer.Write(addressBytes);
                        break;
                    }

                default:
                    throw new ArgumentException("Only IPv4 and IPv6 are allowed.");
            }
        }

        public static MappedAddressAttribute ReadValue(TransactionId transactionId, IDataBufferReader buffer, ushort length)
        {
            buffer.ReadUInt16BigEndian();

            var familyByte = buffer.ReadByte();
            switch (familyByte)
            {
                case 0x01:
                    {
                        var port = buffer.ReadUInt16BigEndian();

                        Span<byte> addressBytes = stackalloc byte[4];
                        buffer.ReadBytes(addressBytes);
                        var address = new IPAddress(addressBytes);

                        return new MappedAddressAttribute(new IPEndPoint(address, port));
                    }

                case 0x02:
                    {
                        var port = buffer.ReadUInt16BigEndian();

                        Span<byte> addressBytes = stackalloc byte[16];
                        buffer.ReadBytes(addressBytes);
                        var address = new IPAddress(addressBytes);

                        return new MappedAddressAttribute(new IPEndPoint(address, port));
                    }

                default:
                    throw new ArgumentException("Only IPv4 and IPv6 are allowed.");
            }
        }
    }
}
