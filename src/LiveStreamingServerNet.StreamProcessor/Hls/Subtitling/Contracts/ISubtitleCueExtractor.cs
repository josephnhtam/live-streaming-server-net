using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt;
using LiveStreamingServerNet.StreamProcessor.Transcriptions;

namespace LiveStreamingServerNet.StreamProcessor.Hls.Subtitling.Contracts
{
    public interface ISubtitleCueExtractor : IAsyncDisposable
    {
        bool RequireTranscribingResult { get; }
        bool RequireTranscribedResult { get; }

        void ReceiveTranscribingResult(TranscribingResult result);
        void ReceiveTranscribedResult(TranscribedResult result);
        bool TryExtractSubtitleCues(TimeSpan segmentStart, ref List<SubtitleCue> cues, out TimeSpan segmentEnd);
    }
}
