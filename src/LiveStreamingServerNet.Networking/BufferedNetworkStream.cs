using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Contracts;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Networking
{
    public class BufferedNetworkStream : INetworkStream
    {
        public Stream InnerStream { get; }

        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private readonly INetBuffer _buffer;
        private readonly Timer _timer;

        public BufferedNetworkStream(Stream stream, NetworkConfiguration config)
        {
            InnerStream = stream;

            _buffer = new NetBuffer(config.SendBufferSize);
            _timer = new Timer(FlushAsync, null, TimeSpan.Zero, config.FlushingInterval);
        }

        public void Dispose()
        {
            InnerStream.Dispose();
            _buffer.Dispose();
            _timer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask ReadExactlyAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            return InnerStream.ReadExactlyAsync(buffer, offset, count, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            await _writeLock.WaitAsync(cancellationToken);

            try
            {
                _buffer.Write(buffer, offset, count);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private async void FlushAsync(object? state)
        {
            if (_buffer.Size == 0)
                return;

            await _writeLock.WaitAsync();

            if (_buffer.Size == 0)
            {
                _writeLock.Release();
                return;
            }

            try
            {
                await InnerStream.WriteAsync(_buffer.UnderlyingBuffer, 0, _buffer.Size);
            }
            catch { }
            finally
            {
                _buffer.Reset();
                _writeLock.Release();
            }
        }
    }
}
