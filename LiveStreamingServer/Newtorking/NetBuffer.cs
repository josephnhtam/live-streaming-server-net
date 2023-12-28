using LiveStreamingServer.Newtorking.Contracts;

namespace LiveStreamingServer.Newtorking
{
    public partial class NetBuffer : INetBuffer
    {
        private readonly MemoryStream _stream;
        private readonly BinaryWriter _writer;
        private readonly BinaryReader _reader;

        public NetBuffer()
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
            _reader = new BinaryReader(_stream);
        }

        public NetBuffer(int initialCapacity)
        {
            _stream = new MemoryStream(initialCapacity);
            _writer = new BinaryWriter(_stream);
            _reader = new BinaryReader(_stream);
        }

        public MemoryStream UnderlyingStream => _stream;
        public int Position
        {
            get => (int)_stream.Position;
            set
            {
                if (_stream.Length < value)
                {
                    _stream.SetLength(value);
                }

                _stream.Position = value;
            }
        }

        private int _size;
        public int Size
        {
            get => _size;
            set
            {
                if (_stream.Length < value)
                {
                    _stream.SetLength(value);
                }

                _size = value;
            }
        }

        public INetBuffer MoveTo(int position)
        {
            Position = position;
            return this;
        }

        public void Reset()
        {
            Position = 0;
            Size = 0;
        }

        public void Flush(INetBuffer output)
        {
            var size = Size;

            Flush(output.UnderlyingStream);
            output.Size += size;
        }

        public void Flush(Stream output)
        {
            var originalLength = _stream.Length;

            _stream.Position = 0;
            _stream.SetLength(Size);
            _stream.CopyTo(output, (int)originalLength);
            if (_stream.CanSeek)
                _stream.Position += Size;

            Reset();
        }

        public void CopyTo(INetBuffer targetBuffer, int bytesCount)
        {
            if (Position + bytesCount > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(bytesCount));
            }

            var targetBufferPosition = targetBuffer.Position;
            targetBuffer.Size += bytesCount;
            _stream.ReadExactly(targetBuffer.UnderlyingStream.GetBuffer(), targetBufferPosition, bytesCount);
            targetBuffer.Position = targetBufferPosition + bytesCount;
        }

        public virtual void Dispose()
        {
            _stream.Dispose();
            _writer.Dispose();
            _reader.Dispose();
        }

        public async Task CopyStreamData(Stream stream, int bytesCount, CancellationToken cancellationToken = default)
        {
            _stream.SetLength(bytesCount);
            await stream.ReadExactlyAsync(_stream.GetBuffer(), 0, bytesCount, cancellationToken);
            Position = 0;
            Size = bytesCount;
        }
    }
}
