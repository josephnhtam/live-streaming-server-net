using System.Buffers;
using System.Security.Cryptography;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal sealed record TransactionId
    {
        private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
        private IMemoryOwner<byte>? _data;

        private int _claimed;

        public ReadOnlySpan<byte> Span => _data != null ? _data.Memory.Span.Slice(0, 12) : [];

        private TransactionId()
        {
            _data = MemoryPool<byte>.Shared.Rent(12);
            _random.GetBytes(_data.Memory.Span.Slice(0, 12));
            _claimed = 1;
        }

        public static TransactionId Create() => new TransactionId();

        public bool Equals(TransactionId? other) =>
            other is not null && Span.SequenceEqual(other.Span);

        public override int GetHashCode() => _data != null ? _data.Memory.GetHashCode() : 0;

        public void Claim(int count = 1)
        {
            Interlocked.Add(ref _claimed, count);
        }

        public void Unclaim(int count = 1)
        {
            IMemoryOwner<byte>? data;

            if (Interlocked.Add(ref _claimed, -count) <= 0 &&
                (data = Interlocked.Exchange(ref _data, null)) != null)
            {
                data.Dispose();
            }
        }
    }
}
