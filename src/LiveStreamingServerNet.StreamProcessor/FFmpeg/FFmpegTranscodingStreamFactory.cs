using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.FFmpeg
{
    public class FFmpegTranscodingStreamFactory : ITranscodingStreamFactory
    {
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IMediaStreamWriterFactory _inputStreamWriterFactory;
        private readonly FFmpegTranscodingStreamConfiguration _config;
        private readonly ILogger<FFmpegTranscodingStream> _logger;

        public FFmpegTranscodingStreamFactory(
            IDataBufferPool dataBufferPool,
            IMediaStreamWriterFactory inputStreamWriterFactory,
            FFmpegTranscodingStreamConfiguration config,
            ILogger<FFmpegTranscodingStream> logger)
        {
            _dataBufferPool = dataBufferPool;
            _inputStreamWriterFactory = inputStreamWriterFactory;
            _config = config;
            _logger = logger;
        }

        public ITranscodingStream Create()
        {
            return new FFmpegTranscodingStream(_dataBufferPool, _inputStreamWriterFactory, _config, _logger);
        }
    }
}
