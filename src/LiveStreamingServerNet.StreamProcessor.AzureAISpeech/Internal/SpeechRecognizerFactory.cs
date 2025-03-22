using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal.Contracts;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal
{
    internal class SpeechRecognizerFactory : ISpeechRecognizerFactory
    {
        private readonly AzureSpeechTranscriptionConfiguration _config;

        public SpeechRecognizerFactory(AzureSpeechTranscriptionConfiguration config)
        {
            _config = config;
        }

        public SpeechRecognizer Create(AudioConfig audioConfig)
        {
            switch (_config)
            {
                case var c when c.AutoDetectLanguageConfig is not null:
                    return new SpeechRecognizer(c.SpeechConfig, c.AutoDetectLanguageConfig, audioConfig);

                case var c when c.SourceLanguageConfig is not null:
                    return new SpeechRecognizer(c.SpeechConfig, c.SourceLanguageConfig, audioConfig);

                case var c when c.Language is not null:
                    return new SpeechRecognizer(c.SpeechConfig, c.Language, audioConfig);

                default:
                    return new SpeechRecognizer(_config.SpeechConfig, audioConfig);
            }
        }
    }
}
