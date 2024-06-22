using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Utilities
{
    internal readonly struct BytesSegments
    {
        private readonly IReadOnlyList<ArraySegment<byte>> _bytes;
        private readonly List<int> _positions;
        private readonly int _length;

        public int Length => _length;

        public BytesSegments(IReadOnlyList<ArraySegment<byte>> bytesSegments)
        {
            _bytes = bytesSegments;

            var positions = new List<int>();
            _positions = positions;

            var pos = 0;
            foreach (var byteSegment in bytesSegments)
            {
                positions.Add(pos);
                pos += byteSegment.Count;
            }

            _length = pos;
        }

        public void WriteTo(IDataBuffer dataBuffer, int offset, int length)
        {
            if (offset < 0 || offset >= _length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < 0 || offset + length > _length)
                throw new ArgumentOutOfRangeException(nameof(length));

            var pos = offset;
            var remaining = length;

            var index = SearchForFirstIndex(pos);

            while (remaining > 0)
            {
                var bytesSegment = _bytes[index];
                var localPos = pos - _positions[index];
                var count = Math.Min(bytesSegment.Count - localPos, remaining);
                dataBuffer.Write(bytesSegment.Array!, bytesSegment.Offset + localPos, count);

                pos += count;
                remaining -= count;
                index++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int SearchForFirstIndex(int pos)
        {
            int left = 0;
            int right = _positions.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;

                if (_positions[mid] <= pos)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return right;
        }
    }
}
