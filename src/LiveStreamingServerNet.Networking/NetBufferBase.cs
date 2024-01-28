using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking
{
    public abstract partial class NetBufferBase : INetBuffer
    {
        public abstract int Position { get; set; }
        public abstract int Size { get; set; }
        public abstract byte[] UnderlyingBuffer { get; }

        public INetBuffer MoveTo(int position)
        {
            Position = position;
            return this;
        }

        private void RefreshSize()
        {
            Size = Math.Max(Size, Position);
        }

        public void Reset()
        {
            Position = 0;
            Size = 0;
        }

        public void Flush(INetBuffer output)
        {
            output.Write(UnderlyingBuffer, 0, Size);
            Reset();
        }

        public void Flush(Stream output)
        {
            output.Write(UnderlyingBuffer, 0, Size);
            Reset();
        }

        public void CopyAllTo(INetBuffer targetBuffer)
        {
            targetBuffer.Write(UnderlyingBuffer, 0, Size);
        }

        public void ReadAndWriteTo(INetBuffer targetBuffer, int bytesCount)
        {
            if (Position + bytesCount > Size)
                throw new ArgumentOutOfRangeException(nameof(bytesCount));

            targetBuffer.Write(UnderlyingBuffer, Position, bytesCount);
            Position += bytesCount;
        }

        public async Task CopyStreamData(Stream stream, int bytesCount, CancellationToken cancellationToken = default)
        {
            Size = bytesCount;
            await stream.ReadExactlyAsync(UnderlyingBuffer, 0, bytesCount, cancellationToken);
            Position = 0;
        }

        protected abstract BinaryWriter GetWriter();
        protected abstract BinaryReader GetReader();

        public virtual void Dispose() { }
    }
}
