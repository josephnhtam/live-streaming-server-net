using LiveStreamingServerNet.StreamProcessor.Configurations;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Services
{
    public class OnDemandStreamCapturer : IOnDemandStreamCapturer
    {
        private readonly IInputPathResolver _inputPathResolver;
        private readonly ILogger<OnDemandStreamCapturer> _logger;
        private readonly OnDemandStreamCapturerConfiguration _config;

        public OnDemandStreamCapturer(IInputPathResolver inputPathResolver, IOptions<OnDemandStreamCapturerConfiguration> config, ILogger<OnDemandStreamCapturer> logger)
        {
            _inputPathResolver = inputPathResolver;
            _config = config.Value;
            _logger = logger;
        }

        public async Task CaptureSnapshotAsync(
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            string outputPath,
            int? height,
            CancellationToken cancellationToken = default)
        {
            var filterArg = BuildVideoFilters(BuildScalingFilter(height));
            var arguments = $"-y -i {{inputPath}} {filterArg} -vframes 1 {{outputPath}}";

            await DoCaptureAsync(streamPath, streamArguments, outputPath, arguments, cancellationToken).ConfigureAwait(false);
        }

        public async Task CaptureClipAsync(
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            string outputPath,
            ClipCaptureOptions options,
            CancellationToken cancellationToken = default)
        {
            var durationSeconds = options.Duration.TotalSeconds;
            var filterArg = BuildVideoFilters(BuildScalingFilter(options.Height), BuildFramerateFilter(options.Framerate));
            var audioOptions = BuildAudioOptions(options.AudioFrequency, options.AudioChannels);
            var arguments = $"-y -t {durationSeconds} -i {{inputPath}} {filterArg} {audioOptions} -loop 0 {{outputPath}}";

            await DoCaptureAsync(streamPath, streamArguments, outputPath, arguments, cancellationToken).ConfigureAwait(false);
        }

        private static string BuildScalingFilter(int? height)
            => height.HasValue ? $"scale=-2:{height.Value}" : string.Empty;

        private static string BuildFramerateFilter(int? framerate)
            => framerate.HasValue ? $"fps={framerate}" : string.Empty;

        private static string BuildVideoFilters(params string[] filters)
        {
            string combined = string.Join(",", filters.Where(f => !string.IsNullOrWhiteSpace(f)));
            return string.IsNullOrWhiteSpace(combined) ? string.Empty : $"-vf {combined}";
        }

        private static string BuildAudioOptions(int? audioFrequency, int? audioChannels)
        {
            var audioOptions = new List<string>();

            if (audioFrequency.HasValue)
                audioOptions.Add($"-ar {audioFrequency.Value}");

            if (audioChannels.HasValue)
                audioOptions.Add($"-ac {audioChannels.Value}");

            return string.Join(" ", audioOptions);
        }

        private async Task DoCaptureAsync(string streamPath, IReadOnlyDictionary<string, string> streamArguments, string outputPath, string ffmpegArgument, CancellationToken cancellation)
        {
            try
            {
                var contextIdentifier = Guid.NewGuid();
                var inputPath = await _inputPathResolver.ResolveInputPathAsync(streamPath, streamArguments).ConfigureAwait(false);

                var ffmpegConfig = new FFmpegProcess.Configuration
                {
                    Name = "StreamCapturer",
                    FFmpegPath = _config.FFmpegPath ?? ExecutableFinder.FindExecutableFromPATH("ffmpeg") ?? throw new ArgumentException("FFmpeg executable not found"),
                    OutputPath = outputPath,
                    Arguments = ffmpegArgument,
                    ContextIdentifier = contextIdentifier,
                    GracefulTerminationSeconds = _config.FFmpegGracefulTerminationSeconds,
                };

                var ffmpegProcess = new FFmpegProcess(streamPath, ffmpegConfig, _logger);
                await ffmpegProcess.RunAsync(inputPath, streamArguments, null, null, cancellation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.StreamCapturingError(streamPath, streamArguments, outputPath, ffmpegArgument, ex);
                throw;
            }
        }
    }
}
