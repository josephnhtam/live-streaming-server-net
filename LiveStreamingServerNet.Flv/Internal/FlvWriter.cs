using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Newtorking.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvWriter : IFlvWriter
    {
        private readonly IFlvClient _client;
        private readonly IStreamWriter _streamWriter;
        private readonly INetBuffer _netBuffer;
        private readonly SemaphoreSlim _syncLock;
        private uint _previousTagSize;

        public FlvWriter(IFlvClient client, IStreamWriter streamWriter)
        {
            _client = client;
            _streamWriter = streamWriter;
            _netBuffer = new NetBuffer();
            _syncLock = new SemaphoreSlim(1, 1);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken)
        {
            byte typeFlags = 0;

            if (allowAudioTags)
                typeFlags |= 0x04;

            if (allowVideoTags)
                typeFlags |= 0x01;

            await _streamWriter.WriteAsync(
                new byte[] { 0x46, 0x4c, 0x56, 0x01, typeFlags, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00 },
                cancellationToken);
        }

        public async Task WriteTagAsync(FlvTagHeader tagHeader, Action<INetBuffer> payloadBufer, CancellationToken cancellationToken)
        {
            try
            {
                await _syncLock.WaitAsync(cancellationToken);

                _netBuffer.WriteUInt32BigEndian(_previousTagSize);

                tagHeader.Write(_netBuffer);

                payloadBufer.Invoke(_netBuffer);

                await _streamWriter.WriteAsync(
                    new ArraySegment<byte>(_netBuffer.UnderlyingStream.GetBuffer(), 0, _netBuffer.Size),
                    cancellationToken);

                _previousTagSize = (uint)(_netBuffer.Size - 4);
            }
            catch (Exception)
            {
                _client.Stop();
            }
            finally
            {
                _netBuffer.Reset();
                _syncLock.Release();
            }
        }

        public ValueTask DisposeAsync()
        {
            _netBuffer.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
