using Microsoft.CognitiveServices.Speech;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal
{
    internal record AzureSpeechTranscriptionConfiguration(SpeechConfig SpeechConfig)
    {
        public string? FFmpegPath { get; init; }
        public string? Language { get; init; }
        public SourceLanguageConfig? SourceLanguageConfig { get; init; }
        public AutoDetectSourceLanguageConfig? AutoDetectLanguageConfig { get; init; }
    };
}
