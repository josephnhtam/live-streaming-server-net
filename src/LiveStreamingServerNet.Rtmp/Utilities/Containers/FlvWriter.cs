using LiveStreamingServerNet.Rtmp.Utilities.Containers.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Nito.AsyncEx;

namespace LiveStreamingServerNet.Rtmp.Utilities.Containers
{
    /// <summary>
    /// Represents a FLV writer.
    /// </summary>
    public class FlvWriter : IFlvWriter
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
                using var _ = await _syncLock.LockAsync(cancellationToken).ConfigureAwait(false);

                byte typeFlags = 0;

                if (allowAudioTags)
                    typeFlags |= 0x04;

                if (allowVideoTags)
                    typeFlags |= 0x01;

                await _streamWriter.WriteAsync(
                    new byte[] {
                        0x46, 0x4c, 0x56, 0x01, typeFlags, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00
                    }, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        }

        public async ValueTask WriteTagAsync(FlvTagType tagType, uint timestamp, Action<IDataBuffer> payloadBuffer, CancellationToken cancellationToken)
        {
            try
            {
                using var _ = await _syncLock.LockAsync(cancellationToken).ConfigureAwait(false);

                var dataBuffer = _dataBufferPool.Obtain();

                try
                {
                    dataBuffer.MoveTo(FlvTagHeader.Size);
                    payloadBuffer.Invoke(dataBuffer);

                    var payloadSize = (uint)(dataBuffer.Size - FlvTagHeader.Size);
                    var packetSize = (uint)dataBuffer.Size;

                    dataBuffer.WriteUInt32BigEndian(packetSize);

                    var header = new FlvTagHeader(tagType, payloadSize, timestamp);
                    header.Write(dataBuffer.MoveTo(0));

                    await _streamWriter.WriteAsync(dataBuffer.AsMemory(), cancellationToken).ConfigureAwait(false);
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

            using var _ = await _syncLock.LockAsync().ConfigureAwait(false);

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
