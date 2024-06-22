using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Nito.AsyncEx;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvWriter : IFlvWriter
    {
        private readonly IStreamWriter _streamWriter;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

        private bool _isDisposed;

        public FlvWriter(IStreamWriter streamWriter, IDataBufferPool dataBufferPool)
        {
            _streamWriter = streamWriter;

            _dataBufferPool = dataBufferPool;
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

        public async ValueTask WriteTagAsync(FlvTagType tagType, uint timestamp, Action<IDataBuffer> payloadBuffer, CancellationToken cancellationToken)
        {
            try
            {
                using var _ = await _syncLock.LockAsync(cancellationToken);

                var dataBuffer = _dataBufferPool.Obtain();

                try
                {
                    dataBuffer.MoveTo(FlvTagHeader.Size);
                    payloadBuffer.Invoke(dataBuffer);

                    var payloadSize = (uint)(dataBuffer.Size - FlvTagHeader.Size);
                    var packageSize = (uint)dataBuffer.Size;

                    dataBuffer.WriteUInt32BigEndian(packageSize);

                    var header = new FlvTagHeader(tagType, payloadSize, timestamp);
                    header.Write(dataBuffer.MoveTo(0));

                    await _streamWriter.WriteAsync(
                        new ArraySegment<byte>(dataBuffer.UnderlyingBuffer, 0, dataBuffer.Size),
                        cancellationToken);
                }
                finally
                {
                    _dataBufferPool.Recycle(dataBuffer);
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
