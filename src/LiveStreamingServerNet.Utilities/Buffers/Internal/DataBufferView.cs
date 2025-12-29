namespace LiveStreamingServerNet.Utilities.Buffers.Internal
{
    internal static class DataBufferView
    {
        public static Span<byte> AsSpan(byte[] buffer, int startIndex, int size)
            => buffer.AsSpan(startIndex, size);

        public static Span<byte> AsSpan(byte[] buffer, int startIndex, int size, int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset > size)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return buffer.AsSpan(startIndex + offset, size - offset);
        }

        public static Span<byte> AsSpan(byte[] buffer, int startIndex, int size, int offset, int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (offset + length > size)
                throw new ArgumentOutOfRangeException(nameof(length));

            return buffer.AsSpan(startIndex + offset, length);
        }

        public static ReadOnlySpan<byte> AsReadOnlySpan(byte[] buffer, int startIndex, int size)
            => buffer.AsSpan(startIndex, size);

        public static ReadOnlySpan<byte> AsReadOnlySpan(byte[] buffer, int startIndex, int size, int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset > size)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return buffer.AsSpan(startIndex + offset, size - offset);
        }

        public static ReadOnlySpan<byte> AsReadOnlySpan(byte[] buffer, int startIndex, int size, int offset, int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (offset + length > size)
                throw new ArgumentOutOfRangeException(nameof(length));

            return buffer.AsSpan(startIndex + offset, length);
        }

        public static Memory<byte> AsMemory(byte[] buffer, int startIndex, int size)
            => buffer.AsMemory(startIndex, size);

        public static Memory<byte> AsMemory(byte[] buffer, int startIndex, int size, int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset > size)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return buffer.AsMemory(startIndex + offset, size - offset);
        }

        public static Memory<byte> AsMemory(byte[] buffer, int startIndex, int size, int offset, int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (offset + length > size)
                throw new ArgumentOutOfRangeException(nameof(length));

            return buffer.AsMemory(startIndex + offset, length);
        }

        public static ReadOnlyMemory<byte> AsReadOnlyMemory(byte[] buffer, int startIndex, int size)
            => buffer.AsMemory(startIndex, size);

        public static ReadOnlyMemory<byte> AsReadOnlyMemory(byte[] buffer, int startIndex, int size, int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset > size)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return buffer.AsMemory(startIndex + offset, size - offset);
        }

        public static ReadOnlyMemory<byte> AsReadOnlyMemory(byte[] buffer, int startIndex, int size, int offset, int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (offset + length > size)
                throw new ArgumentOutOfRangeException(nameof(length));

            return buffer.AsMemory(startIndex + offset, length);
        }


        public static ArraySegment<byte> AsSegment(byte[] buffer, int startIndex, int size)
            => new(buffer, startIndex, size);

        public static ArraySegment<byte> AsSegment(byte[] buffer, int startIndex, int size, int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset > size)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new ArraySegment<byte>(buffer, startIndex + offset, size - offset);
        }

        public static ArraySegment<byte> AsSegment(byte[] buffer, int startIndex, int size, int offset, int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (offset + length > size)
                throw new ArgumentOutOfRangeException(nameof(length));

            return new ArraySegment<byte>(buffer, startIndex + offset, length);
        }
    }
}
