namespace LiveStreamingServerNet.StreamProcessor.Transcriptions
{
    public record struct TranscriptionResult(string ResultId, string SpeakerId, string Text, TimeSpan Timestamp, TimeSpan Duration, List<TranscribedWord>? Words);
    public record struct TranscribedWord(string Text, TimeSpan Timestamp, TimeSpan Duration);
}
