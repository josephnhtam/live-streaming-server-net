using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using Nito.AsyncEx;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvWriter : IFlvWriter
    {
        private readonly IStreamWriter _streamWriter;
        private readonly INetBufferPool _netBufferPool;
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

        private bool _isDisposed;

        public FlvWriter(IStreamWriter streamWriter, INetBufferPool netBufferPool)
        {
            _streamWriter = streamWriter;

            _netBufferPool = netBufferPool;
        }

        public async ValueTask WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken)
        {
            try
            {
                await _syncLock.WaitAsync(cancellationToken);

                byte typeFlags = 0;

                if (allowAudioTags)
                    typeFlags |= 0x04;

                if (allowVideoTags)
                    typeFlags |= 0x01;

                await _streamWriter.WriteAsync(
                    new byte[] {
                        0x46, 0x4c, 0x56, 0x01, typeFlags, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00
                    }, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            finally
            {
                _syncLock.Release();
            }
        }

        public async ValueTask WriteTagAsync(FlvTagType tagType, uint timestamp, Action<INetBuffer> payloadBufer, CancellationToken cancellationToken)
        {
            try
            {
                using var _ = await _syncLock.LockAsync(cancellationToken);

                var netBuffer = _netBufferPool.Obtain();

                try
                {
                    netBuffer.MoveTo(FlvTagHeader.Size);
                    payloadBufer.Invoke(netBuffer);

                    var payloadSize = (uint)(netBuffer.Size - FlvTagHeader.Size);
                    var packageSize = (uint)netBuffer.Size;

                    netBuffer.WriteUInt32BigEndian(packageSize);

                    var header = new FlvTagHeader(tagType, payloadSize, timestamp);
                    header.Write(netBuffer.MoveTo(0));

                    await _streamWriter.WriteAsync(
                        new ArraySegment<byte>(netBuffer.UnderlyingBuffer, 0, netBuffer.Size),
                        cancellationToken);
                }
                finally
                {
                    _netBufferPool.Recycle(netBuffer);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            using var _ = await _syncLock.LockAsync();

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
