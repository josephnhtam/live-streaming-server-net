using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal
{
    internal class AzureSpeechTranscriptionStreamFactory : ITranscriptionStreamFactory
    {
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IConversationTranscriberFactory _transcriberFactory;
        private readonly AzureSpeechTranscriptionConfiguration _config;
        private readonly ILogger<AzureSpeechTranscriptionStream> _transcriptionStreamLogger;
        private readonly ILogger<FFmpegTranscodingStream> _transcodingStreamLogger;

        public AzureSpeechTranscriptionStreamFactory(
            IDataBufferPool dataBufferPool,
            IConversationTranscriberFactory transcriberFactory,
            AzureSpeechTranscriptionConfiguration config,
            ILogger<AzureSpeechTranscriptionStream> transcriptionStreamLogger,
            ILogger<FFmpegTranscodingStream> transcodingStreamLogger)
        {
            _dataBufferPool = dataBufferPool;
            _transcriberFactory = transcriberFactory;
            _config = config;

            _transcriptionStreamLogger = transcriptionStreamLogger;
            _transcodingStreamLogger = transcodingStreamLogger;
        }

        public ITranscriptionStream Create(IMediaStreamWriterFactory inputStreamWriterFactory)
        {
            var transcodingStreamFactory = CreateTranscodingStreamFactory(inputStreamWriterFactory);

            return new AzureSpeechTranscriptionStream(
                _transcriberFactory,
                transcodingStreamFactory,
                _transcriptionStreamLogger
            );
        }

        private ITranscodingStreamFactory CreateTranscodingStreamFactory(IMediaStreamWriterFactory inputStreamWriterFactory)
        {
            var transcodingConfig = CreatePCM16MonoTranscodingConfig(_config.FFmpegPath);
            return new FFmpegTranscodingStreamFactory(_dataBufferPool, inputStreamWriterFactory, transcodingConfig, _transcodingStreamLogger);
        }

        private static FFmpegTranscodingStreamConfiguration CreatePCM16MonoTranscodingConfig(string? ffmpegPath = null)
        {
            return new FFmpegTranscodingStreamConfiguration
            {
                FFmpegPath = ffmpegPath ?? ExecutableFinder.FindExecutableFromPATH("ffmpeg") ??
                    throw new ArgumentException("FFmpeg executable not found"),

                FFmpegArguments = "-i pipe:0 -vn -f s16le -acodec pcm_s16le -ac 1 -ar 16000 pipe:1"
            };
        }
    }
}
