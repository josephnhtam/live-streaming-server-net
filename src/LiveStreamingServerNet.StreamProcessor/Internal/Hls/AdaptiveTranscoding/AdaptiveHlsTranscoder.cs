using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg;
using LiveStreamingServerNet.StreamProcessor.Internal.FFprobe;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.AdaptiveTranscoding
{
    internal partial class AdaptiveHlsTranscoder : IStreamProcessor
    {
        private readonly Configuration _config;
        private readonly ILogger _logger;

        public string Name { get; }
        public Guid ContextIdentifier { get; }

        public AdaptiveHlsTranscoder(Configuration config, ILogger<AdaptiveHlsTranscoder> logger)
        {
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.OutputPath));

            Name = config.Name;
            ContextIdentifier = config.ContextIdentifier;

            _config = config;
            _logger = logger;
        }

        public async Task RunAsync(
            string inputPath,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            OnStreamProcessorStarted? onStarted,
            OnStreamProcessorEnded? onEnded,
            CancellationToken cancellation)
        {
            var streamInfo = await ObtainStreamInformation(inputPath, cancellation);
            await StartTranscoding(inputPath, streamPath, streamArguments, streamInfo, onStarted, onEnded, cancellation);
        }

        private async Task<JsonDocument> ObtainStreamInformation(string inputPath, CancellationToken cancellation)
        {
            var config = new FFprobeProcess.Configuration
            {
                FFprobePath = _config.FFprobePath,
                GracefulTerminationSeconds = _config.FFprobeGracefulShutdownTimeoutSeconds,
                Arguments = "-v quiet -print_format json -show_streams {inputPath}"
            };

            try
            {
                var ffprobeProcess = new FFprobeProcess(config);
                var output = await ffprobeProcess.ExecuteAsync(inputPath, cancellation);
                return JsonDocument.Parse(output);
            }
            catch (Exception ex)
            {
                _logger.ObtainingStreamInformationError(inputPath, ex);
                throw;
            }
        }

        private async Task StartTranscoding(
            string inputPath,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            JsonDocument streamInfo,
            OnStreamProcessorStarted? onStarted,
            OnStreamProcessorEnded? onEnded,
            CancellationToken cancellation)
        {
            var arguments = BuildFFmpegArguments(streamInfo);

            var ffmpegConfig = new FFmpegProcess.Configuration
            {
                ContextIdentifier = ContextIdentifier,
                Name = Name,
                Arguments = arguments,
                FFmpegPath = _config.FFmpegPath,
                OutputPath = _config.OutputPath,
                GracefulTerminationSeconds = _config.FFmpegGracefulTerminationSeconds
            };

            var ffmpegProcess = new FFmpegProcess(ffmpegConfig);
            await ffmpegProcess.RunAsync(inputPath, streamPath, streamArguments, onStarted, onEnded, cancellation);
        }
    }
}
