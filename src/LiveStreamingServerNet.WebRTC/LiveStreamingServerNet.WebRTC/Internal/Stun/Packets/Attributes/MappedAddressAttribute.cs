using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionRequired.MappedAddress)]
    internal record MappedAddressAttribute(IPAddress Address, ushort Port) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.MappedAddress;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
        {
            buffer.Write((byte)0x00);

            switch (Address.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    {
                        buffer.Write((byte)0x01);
                        buffer.WriteUInt16BigEndian(Port);

                        Span<byte> addressBytes = stackalloc byte[4];
                        Address.TryWriteBytes(addressBytes, out _);
                        buffer.Write(addressBytes);
                        break;
                    }

                case AddressFamily.InterNetworkV6:
                    {
                        buffer.Write((byte)0x02);
                        buffer.WriteUInt16BigEndian(Port);

                        Span<byte> addressBytes = stackalloc byte[16];
                        Address.TryWriteBytes(addressBytes, out _);
                        buffer.Write(addressBytes);
                        break;
                    }

                default:
                    throw new ArgumentException("Only IPv4 and IPv6 are allowed.");
            }
        }

        public static MappedAddressAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
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

                        return new MappedAddressAttribute(address, port);
                    }

                case 0x02:
                    {
                        var port = buffer.ReadUInt16BigEndian();

                        Span<byte> addressBytes = stackalloc byte[16];
                        buffer.ReadBytes(addressBytes);
                        var address = new IPAddress(addressBytes);

                        return new MappedAddressAttribute(address, port);
                    }

                default:
                    throw new ArgumentException("Only IPv4 and IPv6 are allowed.");
            }
        }
    }
}
