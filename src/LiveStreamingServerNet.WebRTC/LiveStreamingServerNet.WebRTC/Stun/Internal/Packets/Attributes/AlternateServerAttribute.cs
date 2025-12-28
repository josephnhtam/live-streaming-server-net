using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionOptional.AlternateServer)]
    internal record AlternateServerAttribute(IPEndPoint EndPoint) : IStunAttribute
    {
        public ushort Type => StunAttributeTypes.ComprehensionOptional.AlternateServer;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
        {
            var address = EndPoint.Address;
            var port = (ushort)EndPoint.Port;

            if (address.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 is allowed.");
            }

            Span<byte> addressBytes = stackalloc byte[4];
            address.TryWriteBytes(addressBytes, out _);

            buffer.Write(addressBytes);
            buffer.WriteUInt16BigEndian(port);
            buffer.WriteUInt16BigEndian(0x00);
        }

        public static AlternateServerAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
        {
            Span<byte> addressBytes = stackalloc byte[4];
            buffer.ReadBytes(addressBytes);

            var address = new IPAddress(addressBytes);
            var port = buffer.ReadUInt16BigEndian();
            buffer.ReadUInt16BigEndian();

            return new AlternateServerAttribute(new IPEndPoint(address, port));
        }
    }
}
