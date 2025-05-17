using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt;
using LiveStreamingServerNet.StreamProcessor.Transcriptions;

namespace LiveStreamingServerNet.StreamProcessor.Hls.Subtitling.Contracts
{
    /// <summary>
    /// Defines a contract for extracting subtitle cues from media segments using transcription results.
    /// </summary>
    /// <remarks>
    /// Implement this interface to process transcription results for extracting subtitle cues from media segments.
    /// </remarks>
    public interface ISubtitleCueExtractor : IAsyncDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the extractor requires in-progress transcription results.
        /// </summary>
        bool RequireTranscribingResult { get; }

        /// <summary>
        /// Gets a value indicating whether the extractor requires finalized transcription results.
        /// </summary>
        bool RequireTranscribedResult { get; }

        /// <summary>
        /// Receives an in-progress transcription result.
        /// </summary>
        /// <param name="result">The transcription result produced during the transcription process.</param>
        void ReceiveTranscribingResult(TranscriptionResult result);

        /// <summary>
        /// Receives a finalized transcription result.
        /// </summary>
        /// <param name="result">The completed transcription result produced after processing.</param>
        void ReceiveTranscribedResult(TranscriptionResult result);

        /// <summary>
        /// Attempts to extract subtitle cues from the transcription results starting at the specified time.
        /// </summary>
        /// <param name="segmentStart">The start time from which to extract subtitle cues.</param>
        /// <param name="cues">
        /// A reference to a list of subtitle cues. If subtitle cues are successfully extracted,
        /// they will be added to or updated in this list.
        /// </param>
        /// <param name="segmentEnd">When the extraction is successful, this will contain the end time.</param>
        /// <returns>
        /// <c>true</c> if subtitle cues were successfully extracted; otherwise, <c>false</c>.
        /// </returns>
        bool TryExtractSubtitleCues(TimeSpan segmentStart, ref List<SubtitleCue> cues, out TimeSpan segmentEnd);
    }
}
