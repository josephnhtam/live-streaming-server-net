using LiveStreamingServerNet.Utilities.Buffers.Internal;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public partial class DataBuffer
    {
        public Span<byte> AsSpan()
            => DataBufferView.AsSpan(_buffer, _startIndex, _size);

        public Span<byte> AsSpan(int offset)
            => DataBufferView.AsSpan(_buffer, _startIndex, _size, offset);

        public Span<byte> AsSpan(int offset, int length)
            => DataBufferView.AsSpan(_buffer, _startIndex, _size, offset, length);

        public Memory<byte> AsMemory()
            => DataBufferView.AsMemory(_buffer, _startIndex, _size);

        public Memory<byte> AsMemory(int offset)
            => DataBufferView.AsMemory(_buffer, _startIndex, _size, offset);

        public Memory<byte> AsMemory(int offset, int length)
            => DataBufferView.AsMemory(_buffer, _startIndex, _size, offset, length);

        public ArraySegment<byte> AsSegment()
            => DataBufferView.AsSegment(_buffer, _startIndex, _size);

        public ArraySegment<byte> AsSegment(int offset)
            => DataBufferView.AsSegment(_buffer, _startIndex, _size, offset);

        public ArraySegment<byte> AsSegment(int offset, int length)
            => DataBufferView.AsSegment(_buffer, _startIndex, _size, offset, length);

        public ReadOnlySpan<byte> AsReadOnlySpan()
            => DataBufferView.AsReadOnlySpan(_buffer, _startIndex, _size);

        public ReadOnlySpan<byte> AsReadOnlySpan(int offset)
            => DataBufferView.AsReadOnlySpan(_buffer, _startIndex, _size, offset);

        public ReadOnlySpan<byte> AsReadOnlySpan(int offset, int length)
            => DataBufferView.AsReadOnlySpan(_buffer, _startIndex, _size, offset, length);

        public ReadOnlyMemory<byte> AsReadOnlyMemory()
            => DataBufferView.AsReadOnlyMemory(_buffer, _startIndex, _size);

        public ReadOnlyMemory<byte> AsReadOnlyMemory(int offset)
            => DataBufferView.AsReadOnlyMemory(_buffer, _startIndex, _size, offset);

        public ReadOnlyMemory<byte> AsReadOnlyMemory(int offset, int length)
            => DataBufferView.AsReadOnlyMemory(_buffer, _startIndex, _size, offset, length);
    }
}
