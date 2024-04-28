using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using Nito.AsyncEx;
using System.Threading;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvWriter : IFlvWriter
    {
        private readonly IStreamWriter _streamWriter;
        private readonly INetBufferPool _netBufferPool;
        private readonly AsyncLock _syncLock;

        private bool _isDisposed;

        public FlvWriter(IStreamWriter streamWriter, INetBufferPool netBufferPool)
        {
            _streamWriter = streamWriter;

            _netBufferPool = netBufferPool;
            _syncLock = new AsyncLock();
        }

        public async ValueTask WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken)
        {
            try
            {
                using var _ = await _syncLock.LockAsync(cancellationToken);

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
        }

        public async ValueTask WriteTagAsync(FlvTagHeader tagHeader, Action<INetBuffer> payloadBufer, CancellationToken cancellationToken)
        {
            try
            {
                using var _ = await _syncLock.LockAsync(cancellationToken);

                using var netBuffer = _netBufferPool.Obtain();

                tagHeader.Write(netBuffer);
                payloadBufer.Invoke(netBuffer);
                netBuffer.WriteUInt32BigEndian((uint)netBuffer.Size);

                await _streamWriter.WriteAsync(
                    new ArraySegment<byte>(netBuffer.UnderlyingBuffer, 0, netBuffer.Size),
                    cancellationToken);
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
