using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling
{
    internal class FlvAudioStreamWriterFactory : IMediaStreamWriterFactory
    {
        private readonly IDataBufferPool _dataBufferPool;

        public FlvAudioStreamWriterFactory(IDataBufferPool dataBufferPool)
        {
            _dataBufferPool = dataBufferPool;
        }

        public IMediaStreamWriter Create(IStreamWriter dstStreamWriter)
        {
            return new FlvAudioStreamWriter(dstStreamWriter, _dataBufferPool);
        }
    }
}
