using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal.Contracts
{
    internal interface IConversationTranscriberFactory
    {
        ConversationTranscriber Create(AudioConfig audioConfig);
    }
}
