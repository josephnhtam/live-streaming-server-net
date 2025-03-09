namespace LiveStreamingServerNet.StreamProcessor.Transcriptions
{
    public record struct TranscriptionResult(string Text, TimeSpan Timestamp, TimeSpan Duration);
}
