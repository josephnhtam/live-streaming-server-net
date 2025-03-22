using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal.Contracts
{
    internal interface ISpeechRecognizerFactory
    {
        SpeechRecognizer Create(AudioConfig audioConfig);
    }
}
