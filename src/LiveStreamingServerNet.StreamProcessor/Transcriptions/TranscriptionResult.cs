namespace LiveStreamingServerNet.StreamProcessor.Transcriptions
{
    /// <summary>
    /// Represents a transcription result containing details of recognized speech,
    /// including the transcribed text, speaker information, and timing metadata.
    /// </summary>
    /// <param name="ResultId">A unique identifier for the transcription result.</param>
    /// <param name="SpeakerId">The identifier for the speaker related to the result.</param>
    /// <param name="Text">The complete transcribed text.</param>
    /// <param name="Timestamp">The start timestamp for the transcription result.</param>
    /// <param name="Duration">The duration of the transcription result.</param>
    /// <param name="Words">An optional list of detailed transcribed words, each including its own timing information.</param>
    public record struct TranscriptionResult(
        string ResultId,
        string SpeakerId,
        string Text,
        TimeSpan Timestamp,
        TimeSpan Duration,
        List<TranscribedWord>? Words);

    /// <summary>
    /// Represents a single transcribed word along with its timing metadata.
    /// </summary>
    /// <param name="Text">The recognized word text.</param>
    /// <param name="Timestamp">The timestamp indicating when the word starts.</param>
    /// <param name="Duration">The duration that the word was spoken.</param>
    public record struct TranscribedWord(
        string Text,
        TimeSpan Timestamp,
        TimeSpan Duration);
}
