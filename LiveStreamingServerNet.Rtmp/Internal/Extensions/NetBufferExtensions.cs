using LiveStreamingServerNet.Newtorking.Contracts;
using mtanksl.ActionMessageFormat;

namespace LiveStreamingServerNet.Rtmp.Internal.Extensions
{
    internal static class NetBufferExtensions
    {
        public static ushort ReadUInt16BigEndian(this INetBuffer buffer)
        {
            return (ushort)(buffer.ReadByte() << 8 | buffer.ReadByte());
        }

        public static uint ReadUInt24BigEndian(this INetBuffer buffer)
        {
            return (uint)(buffer.ReadByte() << 16 | buffer.ReadByte() << 8 | buffer.ReadByte());
        }

        public static uint ReadUInt32BigEndian(this INetBuffer buffer)
        {
            return (uint)(buffer.ReadByte() << 24 | buffer.ReadByte() << 16 | buffer.ReadByte() << 8 | buffer.ReadByte());
        }

        public static short ReadInt16BiEndian(this INetBuffer buffer)
        {
            var value = buffer.ReadByte() << 8 | buffer.ReadByte();

            if ((value & 0x8000) != 0)
                value |= unchecked((short)0xffff0000);

            return (short)value;
        }

        public static int ReadInt24BigEndian(this INetBuffer buffer)
        {
            var value = buffer.ReadByte() << 16 | buffer.ReadByte() << 8 | buffer.ReadByte();

            if ((value & 0x800000) != 0)
                value |= unchecked((int)0xff000000);

            return value;
        }

        public static int ReadInt32BigEndian(this INetBuffer buffer)
        {
            var value = buffer.ReadByte() << 24 | buffer.ReadByte() << 16 | buffer.ReadByte() << 8 | buffer.ReadByte();

            if ((value & 0x80000000) != 0)
                value |= unchecked((int)0xff000000);

            return value;
        }

        public static void WriteUint16BigEndian(this INetBuffer buffer, ushort value)
        {
            buffer.Write((byte)(value >> 8));
            buffer.Write((byte)value);
        }

        public static void WriteUInt24BigEndian(this INetBuffer buffer, uint value)
        {
            buffer.Write((byte)(value >> 16));
            buffer.Write((byte)(value >> 8));
            buffer.Write((byte)value);
        }

        public static void WriteUInt32BigEndian(this INetBuffer buffer, uint value)
        {
            buffer.Write((byte)(value >> 24));
            buffer.Write((byte)(value >> 16));
            buffer.Write((byte)(value >> 8));
            buffer.Write((byte)value);
        }

        public static void WriteInt16BigEndian(this INetBuffer buffer, short value)
        {
            buffer.Write((byte)(value >> 8));
            buffer.Write((byte)value);
        }

        public static void WriteInt24BigEndian(this INetBuffer buffer, int value)
        {
            buffer.Write((byte)(value >> 16));
            buffer.Write((byte)(value >> 8));
            buffer.Write((byte)value);
        }

        public static void WriteInt32BigEndian(this INetBuffer buffer, int value)
        {
            buffer.Write((byte)(value >> 24));
            buffer.Write((byte)(value >> 16));
            buffer.Write((byte)(value >> 8));
            buffer.Write((byte)value);
        }

        public static void WriteAmf(this INetBuffer buffer, IList<object?> values, AmfEncodingType type)
        {
            var writer = new AmfWriter();

            foreach (var value in values)
            {
                if (value is IDictionary<string, object?> map)
                {
                    if (type == AmfEncodingType.Amf0)
                        writer.WriteAmf0ObjectWithType(map);
                    else
                        writer.WriteAmf3ObjectWithType(map);

                    continue;
                }

                if (type == AmfEncodingType.Amf0)
                    writer.WriteAmf0(value);
                else
                    writer.WriteAmf3(value);
            }

            buffer.Write(writer.Data);
        }

        public static object[] ReadAmf(this INetBuffer buffer, int bytesCount, int valuesCount, AmfEncodingType type)
        {
            var data = buffer.ReadBytes(bytesCount);
            var reader = new AmfReader(data);
            var results = new object[valuesCount];

            try
            {
                for (int i = 0; i < valuesCount; i++)
                {
                    var parameter = type == AmfEncodingType.Amf3 ? reader.ReadAmf3() : reader.ReadAmf0();

                    if (parameter is IAmfObject amfObject)
                        parameter = amfObject.ToObject();

                    results[i] = parameter;
                }
            }
            catch (IndexOutOfRangeException) { }

            return results;
        }

        public static IList<object> ReadAmf(this INetBuffer buffer, int bytesCount, AmfEncodingType type)
        {
            var data = buffer.ReadBytes(bytesCount);
            var reader = new AmfReader(data);
            var results = new List<object>();

            try
            {
                while (true)
                {
                    var parameter = type == AmfEncodingType.Amf3 ? reader.ReadAmf3() : reader.ReadAmf0();

                    if (parameter is IAmfObject amfObject)
                        parameter = amfObject.ToObject();

                    results.Add(parameter);
                }
            }
            catch (IndexOutOfRangeException) { }

            return results;
        }
    }

    internal enum AmfEncodingType
    {
        Amf0,
        Amf3
    }
}
