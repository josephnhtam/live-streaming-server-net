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
    /// <summary>
    /// Provides extension methods for configuring Azure Speech Transcription within the HLS transmuxer.
    /// </summary>
    public static class AzureSpeechTranscriptionInstaller
    {
        /// <summary>
        /// Adds Azure Speech Transcription to the HLS transmuxer configurator using the specified subtitle track options and speech configuration.
        /// </summary>
        /// <param name="configurator">The HLS transmuxer configurator instance.</param>
        /// <param name="options">Subtitle track options for the transcription.</param>
        /// <param name="speechConfig">The speech configuration for Azure Speech.</param>
        /// <returns>The updated HLS transmuxer configurator instance.</returns>
        public static IHlsTransmuxerConfigurator AddAzureSpeechTranscription(
            this IHlsTransmuxerConfigurator configurator,
            SubtitleTrackOptions options,
            SpeechConfig speechConfig)
        {
            return AddAzureSpeechTranscription(configurator, options, speechConfig, configureAzureSpeech: null, configureSubtitleTranscription: null);
        }

        /// <summary>
        /// Adds Azure Speech Transcription to the HLS transmuxer configurator with additional configuration for Azure Speech Transcription.
        /// </summary>
        /// <param name="configurator">The HLS transmuxer configurator instance.</param>
        /// <param name="options">Subtitle track options for the transcription.</param>
        /// <param name="speechConfig">The speech configuration for Azure Speech.</param>
        /// <param name="configure">An action to configure the Azure Speech Transcription.</param>
        /// <returns>The updated HLS transmuxer configurator instance.</returns>
        public static IHlsTransmuxerConfigurator AddAzureSpeechTranscription(
           this IHlsTransmuxerConfigurator configurator,
           SubtitleTrackOptions options,
           SpeechConfig speechConfig,
           Action<IAzureSpeechTranscriptionConfigurator>? configure)
        {
            return AddAzureSpeechTranscription(configurator, options, speechConfig, configureAzureSpeech: configure, configureSubtitleTranscription: null);
        }

        /// <summary>
        /// Adds Azure Speech Transcription to the HLS transmuxer configurator with additional configurations for both Azure Speech Transcription and Subtitle Transcription.
        /// </summary>
        /// <param name="configurator">The HLS transmuxer configurator instance.</param>
        /// <param name="options">Subtitle track options for the transcription.</param>
        /// <param name="speechConfig">The speech configuration for Azure Speech.</param>
        /// <param name="configureAzureSpeech">An action to configure Azure Speech Transcription.</param>
        /// <param name="configureSubtitleTranscription">An action to configure Subtitle Transcription.</param>
        /// <returns>The updated HLS transmuxer configurator instance.</returns>
        public static IHlsTransmuxerConfigurator AddAzureSpeechTranscription(
            this IHlsTransmuxerConfigurator configurator,
            SubtitleTrackOptions options,
            SpeechConfig speechConfig,
            Action<IAzureSpeechTranscriptionConfigurator>? configureAzureSpeech,
            Action<ISubtitleTranscriptionConfigurator>? configureSubtitleTranscription)
        {
            var transcriptionConfigurator = new AzureSpeechTranscriptionConfigurator(speechConfig);
            configureAzureSpeech?.Invoke(transcriptionConfigurator);

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

            return configurator.AddSubtitleTranscription(options, transcriptionStreamFactory, configureSubtitleTranscription);
        }
    }
}