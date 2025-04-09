using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal.Contracts;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal
{
    internal class ConversationTranscriberFactory : IConversationTranscriberFactory
    {
        private readonly AzureSpeechTranscriptionConfiguration _config;

        public ConversationTranscriberFactory(AzureSpeechTranscriptionConfiguration config)
        {
            _config = config;
        }

        public ConversationTranscriber Create(AudioConfig audioConfig)
        {
            switch (_config)
            {
                case var c when c.AutoDetectLanguageConfig is not null:
                    return new ConversationTranscriber(c.SpeechConfig, c.AutoDetectLanguageConfig, audioConfig);

                case var c when c.SourceLanguageConfig is not null:
                    return new ConversationTranscriber(c.SpeechConfig, c.SourceLanguageConfig, audioConfig);

                default:
                    return new ConversationTranscriber(_config.SpeechConfig, audioConfig);
            }
        }
    }
}
