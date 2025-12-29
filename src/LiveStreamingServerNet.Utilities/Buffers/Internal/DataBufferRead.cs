using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace LiveStreamingServerNet.Utilities.Buffers.Internal
{
    internal static class DataBufferRead
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureRemainingSize(int size, int position, int length)
        {
            if (position + length > size)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public static void ReadBytes(byte[] source, int startIndex, int size, ref int position, byte[] buffer, int index, int count)
        {
            ReadBytes(source, startIndex, size, ref position, buffer.AsSpan(index, count));
        }

        public static void ReadBytes(byte[] source, int startIndex, int size, ref int position, Span<byte> buffer)
        {
            var bufferLength = buffer.Length;
            EnsureRemainingSize(size, position, bufferLength);

            source.AsSpan(startIndex + position, bufferLength).CopyTo(buffer);
            position += bufferLength;
        }

        public static byte[] ReadBytes(byte[] source, int startIndex, int size, ref int position, int count)
        {
            var result = new byte[count];
            ReadBytes(source, startIndex, size, ref position, result.AsSpan());
            return result;
        }

        public static byte ReadByte(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 1);

            return source[startIndex + position++];
        }

        public static short ReadInt16(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 2);

            var value = Unsafe.As<byte, short>(ref source[startIndex + position]);
            position += 2;
            return value;
        }

        public static int ReadInt32(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 4);

            var value = Unsafe.As<byte, int>(ref source[startIndex + position]);
            position += 4;
            return value;
        }

        public static long ReadInt64(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 8);

            var value = Unsafe.As<byte, long>(ref source[startIndex + position]);
            position += 8;
            return value;
        }

        public static ushort ReadUInt16(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 2);

            var value = Unsafe.As<byte, ushort>(ref source[startIndex + position]);
            position += 2;
            return value;
        }

        public static uint ReadUInt32(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 4);

            var value = Unsafe.As<byte, uint>(ref source[startIndex + position]);
            position += 4;
            return value;
        }

        public static ulong ReadUInt64(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 8);

            var value = Unsafe.As<byte, ulong>(ref source[startIndex + position]);
            position += 8;
            return value;
        }

        public static float ReadSingle(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 4);

            var value = Unsafe.As<byte, float>(ref source[startIndex + position]);
            position += 4;
            return value;
        }

        public static double ReadDouble(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 8);

            var value = Unsafe.As<byte, double>(ref source[startIndex + position]);
            position += 8;
            return value;
        }

        public static bool ReadBoolean(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 1);

            return Unsafe.As<byte, bool>(ref source[startIndex + position++]);
        }

        public static char ReadChar(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 2);

            var value = Unsafe.As<byte, char>(ref source[startIndex + position]);
            position += 2;
            return value;
        }

        public static ushort ReadUInt16BigEndian(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 2);

            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, ushort>(ref source[startIndex + position]));
            position += 2;
            return value;
        }

        public static uint ReadUInt24BigEndian(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 3);

            var value =
                source[startIndex + position] << 16 |
                source[startIndex + position + 1] << 8 |
                source[startIndex + position + 2];

            position += 3;
            return (uint)value;
        }

        public static uint ReadUInt32BigEndian(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 4);

            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, uint>(ref source[startIndex + position]));
            position += 4;
            return value;
        }

        public static ulong ReadUInt64BigEndian(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 8);

            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, ulong>(ref source[startIndex + position]));
            position += 8;
            return value;
        }

        public static short ReadInt16BigEndian(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 2);

            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, short>(ref source[startIndex + position]));
            position += 2;
            return value;
        }

        public static int ReadInt24BigEndian(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 3);

            var value =
                source[startIndex + position] << 16 |
                source[startIndex + position + 1] << 8 |
                source[startIndex + position + 2];

            if ((value & 0x800000) != 0)
                value |= unchecked((int)0xff000000);

            position += 3;
            return value;
        }

        public static int ReadInt32BigEndian(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 4);

            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, int>(ref source[startIndex + position]));
            position += 4;
            return value;
        }

        public static long ReadInt64BigEndian(byte[] source, int startIndex, int size, ref int position)
        {
            EnsureRemainingSize(size, position, 8);

            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, long>(ref source[startIndex + position]));
            position += 8;
            return value;
        }

        public static string ReadUtf8String(byte[] source, int startIndex, int size, ref int position, int length)
        {
            EnsureRemainingSize(size, position, length);

            var targetSpan = source.AsSpan(startIndex + position, length);
            var result = Encoding.UTF8.GetString(targetSpan);
            position += length;
            return result;
        }
    }
}
