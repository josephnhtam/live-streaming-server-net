using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Newtorking
{
    public class NetBuffer : NetBufferBase
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

        public override byte[] UnderlyingBuffer => _stream.GetBuffer();

        public override int Position
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
        public override int Size
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override BinaryWriter GetWriter() => _writer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override BinaryReader GetReader() => _reader;

        public override void Dispose()
        {
            _stream.Dispose();
            _writer.Dispose();
            _reader.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
