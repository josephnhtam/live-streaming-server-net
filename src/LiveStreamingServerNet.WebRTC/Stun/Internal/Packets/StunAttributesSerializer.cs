using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets
{
    internal static class StunAttributesSerializer
    {
        public static int Write(IDataBuffer buffer, TransactionId transactionId, IReadOnlyList<IStunAttribute> attributes)
        {
            var start = buffer.Position;

            foreach (var attribute in attributes)
            {
                buffer.WriteUInt16BigEndian(attribute.Type);

                var valueLengthPos = buffer.Position;
                buffer.Advance(2);

                var valueStart = buffer.Position;
                attribute.WriteValue(transactionId, buffer);

                var valueEnd = buffer.Position;
                var valueLength = (ushort)(valueEnd - valueStart);

                buffer.MoveTo(valueLengthPos);
                buffer.WriteUInt16BigEndian(valueLength);
                buffer.MoveTo(valueEnd);

                var paddingBytes = CalculatePaddingBytes(valueLength);
                for (var i = 0; i < paddingBytes; i++)
                {
                    buffer.Write((byte)0);
                }
            }

            return buffer.Position - start;
        }

        public static (List<IStunAttribute> attributes, UnknownAttributes? unknownAttributes) Read(IDataBufferReader buffer, TransactionId transactionId, int bodyLength)
        {
            UnknownAttributes? unknownAttributes = null;

            var attributes = new List<IStunAttribute>();
            var start = buffer.Position;

            while (buffer.Position < start + bodyLength)
            {
                var type = buffer.ReadUInt16BigEndian();
                var valueLength = buffer.ReadUInt16BigEndian();

                var attributeStart = buffer.Position;

                var attribute = StunAttributesRegistry.ReadAttributeValue(transactionId, type, valueLength, buffer);
                if (attribute != null)
                {
                    attributes.Add(attribute);

                    if (buffer.Position != attributeStart + valueLength)
                    {
                        throw new InvalidDataException("STUN attribute value length mismatch.");
                    }
                }
                else
                {
                    unknownAttributes ??= new UnknownAttributes();
                    unknownAttributes.Add(type);

                    buffer.Position += valueLength;
                }

                var paddingBytes = CalculatePaddingBytes(valueLength);
                buffer.Position += paddingBytes;
            }

            return (attributes, unknownAttributes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalculatePaddingBytes(int length)
        {
            return (4 - (length % 4)) % 4;
        }
    }
}
