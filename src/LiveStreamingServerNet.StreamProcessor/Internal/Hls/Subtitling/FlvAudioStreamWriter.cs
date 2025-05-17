using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Utilities.Containers;
using LiveStreamingServerNet.Rtmp.Utilities.Containers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling
{
    internal class FlvAudioStreamWriter : IMediaStreamWriter
    {
        private readonly IFlvWriter _flvWriter;

        public FlvAudioStreamWriter(IStreamWriter streamWriter, IDataBufferPool dataBufferPool)
        {
            _flvWriter = new FlvWriter(streamWriter, dataBufferPool);
        }

        public ValueTask WriteHeaderAsync(CancellationToken cancellationToken)
        {
            return _flvWriter.WriteHeaderAsync(true, false, cancellationToken);
        }

        public async ValueTask WriteBufferAsync(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken)
        {
            if (mediaType != MediaType.Audio)
            {
                throw new ArgumentException("Invalid media type", nameof(mediaType));
            }

            rentedBuffer.Claim();

            try
            {
                await _flvWriter.WriteTagAsync(FlvTagType.Audio, timestamp,
                    buffer => buffer.Write(rentedBuffer.AsSpan()), cancellationToken);
            }
            finally
            {
                rentedBuffer.Unclaim();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _flvWriter.DisposeAsync();
        }
    }
}
