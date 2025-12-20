using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes
{
    internal record MappedAddressAttribute(IPAddress Address, ushort Port) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.MappedAddress;

        public void Write(BindingRequest request, IDataBuffer buffer)
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
                    throw new NotSupportedException("Only IPv4 and IPv6 are supported.");
            }
        }
    }
}
