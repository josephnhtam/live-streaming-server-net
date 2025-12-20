using System.Buffers;
using System.Security.Cryptography;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages
{
    internal sealed record TransactionId : IEquatable<TransactionId>, IDisposable
    {
        private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
        private readonly IMemoryOwner<byte> _data;

        public ReadOnlySpan<byte> Span => _data.Memory.Span.Slice(0, 12);

        private TransactionId()
        {
            _data = MemoryPool<byte>.Shared.Rent(12);
            _random.GetBytes(_data.Memory.Span.Slice(0, 12));
        }

        public static TransactionId Create() => new TransactionId();

        public bool Equals(TransactionId? other) =>
            other is not null && Span.SequenceEqual(other.Span);

        public override int GetHashCode() => _data.Memory.GetHashCode();

        public void Dispose() => _data.Dispose();
    }
}
