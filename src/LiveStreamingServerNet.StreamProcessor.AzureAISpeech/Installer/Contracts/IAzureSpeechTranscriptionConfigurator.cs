using Microsoft.CognitiveServices.Speech;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Installer.Contracts
{
    /// <summary>
    /// Defines a configurator for Azure Speech Transcription settings.
    /// Provides methods for specifying the FFmpeg path and language configuration options.
    /// </summary>
    public interface IAzureSpeechTranscriptionConfigurator
    {
        /// <summary>
        /// Specifies the path to the FFmpeg executable used to transcode audio streams.
        /// </summary>
        /// <param name="path">The file system path to the FFmpeg executable.</param>
        /// <returns>
        /// The current instance of <see cref="IAzureSpeechTranscriptionConfigurator"/> for method chaining.
        /// </returns>
        IAzureSpeechTranscriptionConfigurator WithFFmpegPath(string path);

        /// <summary>
        /// Configures the transcription with the source language settings.
        /// </summary>
        /// <param name="config">
        /// An instance of <see cref="SourceLanguageConfig"/> that specifies the source language configuration.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="IAzureSpeechTranscriptionConfigurator"/> for method chaining.
        /// </returns>
        IAzureSpeechTranscriptionConfigurator WithSourceLanguageConfig(SourceLanguageConfig config);

        /// <summary>
        /// Configures the transcription to automatically detect the source language.
        /// </summary>
        /// <param name="config">
        /// An instance of <see cref="AutoDetectSourceLanguageConfig"/> that specifies the auto-detect language configuration.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="IAzureSpeechTranscriptionConfigurator"/> for method chaining.
        /// </returns>
        IAzureSpeechTranscriptionConfigurator WithAutoDetectLanguageConfig(AutoDetectSourceLanguageConfig config);
    }
}