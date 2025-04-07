namespace LiveStreamingServerNet.StreamProcessor.Transcriptions
{
    public record struct TranscribingResult(string Text, TimeSpan Timestamp, TimeSpan Duration);
    public record struct TranscribedResult(string Text, TimeSpan Timestamp, TimeSpan Duration, List<TranscribedWord>? Words);
    public record struct TranscribedWord(string Text, TimeSpan Timestamp, TimeSpan Duration);
}
