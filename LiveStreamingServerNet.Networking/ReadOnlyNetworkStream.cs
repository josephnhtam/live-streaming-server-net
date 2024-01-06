using System.Net.Sockets;

namespace LiveStreamingServerNet.Newtorking
{
    public sealed class ReadOnlyNetworkStream : Stream
    {
        private readonly NetworkStream _innerStream;

        public ReadOnlyNetworkStream(NetworkStream innerStream)
        {
            _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        }

        public override bool CanWrite => false;
        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanTimeout => _innerStream.CanTimeout;
        public override long Length => _innerStream.Length;
        public override long Position { get => _innerStream.Position; set => _innerStream.Position = value; }
        public override int ReadTimeout { get => _innerStream.ReadTimeout; set => _innerStream.ReadTimeout = value; }
        public override int WriteTimeout { get => _innerStream.WriteTimeout; set => _innerStream.WriteTimeout = value; }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return _innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return _innerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            _innerStream.Close();
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            _innerStream.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override ValueTask DisposeAsync()
        {
            return _innerStream.DisposeAsync();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _innerStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _innerStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _innerStream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            return _innerStream.Read(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _innerStream.ReadAsync(buffer, cancellationToken);
        }

        public override int ReadByte()
        {
            return _innerStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override string? ToString()
        {
            return _innerStream.ToString();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _innerStream.Write(buffer);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _innerStream.WriteAsync(buffer, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            _innerStream.WriteByte(value);
        }

        protected override void Dispose(bool disposing)
        {
            _innerStream.Dispose();
        }
    }
}
