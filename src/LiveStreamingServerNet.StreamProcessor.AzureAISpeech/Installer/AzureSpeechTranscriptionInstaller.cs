using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal;
using LiveStreamingServerNet.StreamProcessor.FFmpeg;
using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Installer
{
    public static class AzureSpeechTranscriptionInstaller
    {
        public static IHlsTransmuxerConfigurator AddAzureSpeechTranscription(
            this IHlsTransmuxerConfigurator configurator,
            SubtitleTrackOptions options,
            SpeechConfig speechConfig)
        {
            return AddAzureSpeechTranscription(configurator, options, speechConfig, null);
        }

        public static IHlsTransmuxerConfigurator AddAzureSpeechTranscription(
            this IHlsTransmuxerConfigurator configurator,
            SubtitleTrackOptions options,
            SpeechConfig speechConfig,
            Action<IAzureSpeechTranscriptionConfigurator>? configure)
        {
            var transcriptionConfigurator = new AzureSpeechTranscriptionConfigurator(speechConfig);
            configure?.Invoke(transcriptionConfigurator);

            var transcriptionConfig = transcriptionConfigurator.Build();
            var speechRecognizerFactory = new ConversationTranscriberFactory(transcriptionConfig);

            Func<IServiceProvider, ITranscriptionStreamFactory> transcriptionStreamFactory = svc =>
            {
                var dataBufferPool = svc.GetRequiredService<IDataBufferPool>();
                var transcriptionStreamLogger = svc.GetRequiredService<ILogger<AzureSpeechTranscriptionStream>>();
                var transcodingStreamLogger = svc.GetRequiredService<ILogger<FFmpegTranscodingStream>>();

                return new AzureSpeechTranscriptionStreamFactory(
                    dataBufferPool, speechRecognizerFactory, transcriptionConfig, transcriptionStreamLogger, transcodingStreamLogger);
            };

            return transcriptionConfig.SubtitleCueExtractorFactory == null ?
                configurator.AddSubtitleTranscriptionStreamFactory(options, transcriptionStreamFactory) :
                configurator.AddSubtitleTranscriptionStreamFactory(options, transcriptionStreamFactory, transcriptionConfig.SubtitleCueExtractorFactory);
        }
    }
}
