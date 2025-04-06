using Microsoft.CognitiveServices.Speech;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Installer.Contracts
{
    public interface IAzureSpeechTranscriptionConfigurator
    {
        IAzureSpeechTranscriptionConfigurator WithFFmpegPath(string path);
        IAzureSpeechTranscriptionConfigurator WithSourceLanguageConfig(SourceLanguageConfig config);
        IAzureSpeechTranscriptionConfigurator WithAutoDetectLanguageConfig(AutoDetectSourceLanguageConfig config);
    }
}
