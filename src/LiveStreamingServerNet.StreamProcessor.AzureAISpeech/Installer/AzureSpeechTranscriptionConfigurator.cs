using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal;
using Microsoft.CognitiveServices.Speech;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Installer
{
    internal class AzureSpeechTranscriptionConfigurator : IAzureSpeechTranscriptionConfigurator
    {
        private AzureSpeechTranscriptionConfiguration _config;

        public AzureSpeechTranscriptionConfigurator(SpeechConfig speechConfig)
        {
            _config = new AzureSpeechTranscriptionConfiguration(speechConfig);
        }

        public IAzureSpeechTranscriptionConfigurator WithFFmpegPath(string path)
        {
            _config = _config with { FFmpegPath = path };
            return this;
        }

        public IAzureSpeechTranscriptionConfigurator WithLanguage(string language)
        {
            _config = _config with { Language = language };
            return this;
        }

        public IAzureSpeechTranscriptionConfigurator WithSourceLanguageConfig(SourceLanguageConfig config)
        {
            _config = _config with { SourceLanguageConfig = config };
            return this;
        }

        public IAzureSpeechTranscriptionConfigurator WithAutoDetectLanguageConfig(AutoDetectSourceLanguageConfig config)
        {
            _config = _config with { AutoDetectLanguageConfig = config };
            return this;
        }

        public AzureSpeechTranscriptionConfiguration Build()
        {
            return _config;
        }
    }
}
