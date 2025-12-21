using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionOptional.AlternateServer)]
    internal record AlternateServerAttribute(IPAddress Address, ushort Port) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionOptional.AlternateServer;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
        {
            if (Address.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 is allowed.");
            }

            Span<byte> addressBytes = stackalloc byte[4];
            Address.TryWriteBytes(addressBytes, out _);

            buffer.Write(addressBytes);
            buffer.WriteUInt16BigEndian(Port);
            buffer.WriteUInt16BigEndian(0x00);
        }

        public static AlternateServerAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
        {
            Span<byte> addressBytes = stackalloc byte[4];
            buffer.ReadBytes(addressBytes);

            var address = new IPAddress(addressBytes);
            var port = buffer.ReadUInt16BigEndian();
            buffer.ReadUInt16BigEndian();

            return new AlternateServerAttribute(address, port);
        }
    }
}
