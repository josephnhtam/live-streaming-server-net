using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Logging;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvWriter : IFlvWriter
    {
        private readonly ILogger _logger;
        private readonly INetBuffer _netBuffer;
        private readonly SemaphoreSlim _syncLock;

        private IFlvClient _client = default!;
        private IStreamWriter _streamWriter = default!;

        public FlvWriter(ILogger<FlvWriter> logger)
        {
            _logger = logger;

            _netBuffer = new NetBuffer();
            _syncLock = new SemaphoreSlim(1, 1);
        }

        public void Initialize(IFlvClient client, IStreamWriter streamWriter)
        {
            _client = client;
            _streamWriter = streamWriter;
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
            catch (Exception ex)
            {
                _logger.FailedToWriteFlvHeader(_client.ClientId, ex);
                _client.Stop();
            }
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
            catch (Exception ex)
            {
                _logger.FailedToWriteFlvTag(_client.ClientId, ex);
                _client.Stop();
            }
            finally
            {
                _netBuffer.Reset();
                _syncLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _syncLock.WaitAsync();
                _netBuffer.Dispose();
            }
            finally
            {
                _syncLock.Release();
            }
        }
    }
}
