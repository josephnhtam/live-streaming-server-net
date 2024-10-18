using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Streams.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Streams
{
    internal class RtmpDownstreamProcessFactory : IRtmpDownstreamProcessFactory
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpStreamDeletionService _streamDeletion;
        private readonly IRtmpVideoDataProcessorService _videoDataProcessor;
        private readonly IRtmpAudioDataProcessorService _audioDataProcessor;
        private readonly IRtmpMetaDataProcessorService _metaDataProcessor;
        private readonly IRtmpOriginResolver _originResolver;
        private readonly IBufferPool _bufferPool;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IOptions<RtmpDownstreamConfiguration> _config;
        private readonly ILogger<RtmpDownstreamProcess> _logger;

        public RtmpDownstreamProcessFactory(
            IRtmpStreamManagerService streamManager,
            IRtmpStreamDeletionService streamDeletion,
            IRtmpVideoDataProcessorService videoDataProcessor,
            IRtmpAudioDataProcessorService audioDataProcessor,
            IRtmpMetaDataProcessorService metaDataProcessor,
            IRtmpOriginResolver originResolver,
            IBufferPool bufferPool,
            IDataBufferPool dataBufferPool,
            IOptions<RtmpDownstreamConfiguration> config,
            ILogger<RtmpDownstreamProcess> logger)
        {
            _streamManager = streamManager;
            _streamDeletion = streamDeletion;
            _videoDataProcessor = videoDataProcessor;
            _audioDataProcessor = audioDataProcessor;
            _metaDataProcessor = metaDataProcessor;
            _originResolver = originResolver;
            _bufferPool = bufferPool;
            _dataBufferPool = dataBufferPool;
            _config = config;
            _logger = logger;
        }

        public IRtmpDownstreamProcess Create(string streamPath)
        {
            return new RtmpDownstreamProcess(
                streamPath,
                _streamManager,
                _streamDeletion,
                _videoDataProcessor,
                _audioDataProcessor,
                _metaDataProcessor,
                _originResolver,
                _bufferPool,
                _dataBufferPool,
                _config,
                _logger);
        }
    }
}
