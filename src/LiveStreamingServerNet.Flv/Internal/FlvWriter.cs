using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvWriter : IFlvWriter
    {
        private readonly IStreamWriter _streamWriter;
        private readonly INetBuffer _netBuffer;
        private readonly SemaphoreSlim _syncLock;

        private bool _isDisposed;

        public FlvWriter(IStreamWriter streamWriter)
        {
            _streamWriter = streamWriter;

            _netBuffer = new NetBuffer();
            _syncLock = new SemaphoreSlim(1, 1);
        }

        public async ValueTask WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken)
        {
            try
            {
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
                await _syncLock.WaitAsync(cancellationToken);

                tagHeader.Write(_netBuffer);
                payloadBufer.Invoke(_netBuffer);
                _netBuffer.WriteUInt32BigEndian((uint)_netBuffer.Size);

                await _streamWriter.WriteAsync(
                    new ArraySegment<byte>(_netBuffer.UnderlyingBuffer, 0, _netBuffer.Size),
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            finally
            {
                _netBuffer.Reset();
                _syncLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            try
            {
                await _syncLock.WaitAsync();
                _netBuffer.Dispose();
            }
            finally
            {
                _syncLock.Release();
            }

            GC.SuppressFinalize(this);
        }
    }
}
