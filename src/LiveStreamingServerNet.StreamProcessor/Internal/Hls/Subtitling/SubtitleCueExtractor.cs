using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt;
using LiveStreamingServerNet.StreamProcessor.Transcriptions;
using System.Threading.Channels;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling
{
    internal class SubtitleCueExtractor : ISubtitleCueExtractor
    {
        private readonly Channel<TranscribingResult> _transcriptionResults;

        public bool RequireTranscribingResult => true;
        public bool RequireTranscribedResult => false;

        public SubtitleCueExtractor()
        {
            _transcriptionResults = Channel.CreateUnbounded<TranscribingResult>(new() { AllowSynchronousContinuations = true });
        }

        public void ReceiveTranscribingResult(TranscribingResult result)
        {
            _transcriptionResults.Writer.TryWrite(result);
        }

        public void ReceiveTranscribedResult(TranscribedResult result) { }

        public bool TryExtractSubtitleCues(TimeSpan segmentStart, ref List<SubtitleCue> cues, out TimeSpan segmentEnd)
        {
            cues.Clear();
            var nextCueTimestamp = segmentStart;

            while (_transcriptionResults.Reader.TryRead(out var transcription))
            {
                var transcriptionEnd = transcription.Timestamp + transcription.Duration;
                var cueStart = transcription.Timestamp > nextCueTimestamp ? transcription.Timestamp : nextCueTimestamp;
                var cueDuration = transcriptionEnd - cueStart;

                if (cueDuration <= TimeSpan.Zero)
                {
                    continue;
                }

                cues.Add(new SubtitleCue(transcription.Text, cueStart, cueDuration));
                nextCueTimestamp = cueStart + cueDuration;
            }

            segmentEnd = nextCueTimestamp;
            return cues.Count > 0;
        }

        public ValueTask DisposeAsync()
        {
            _transcriptionResults.Writer.Complete();
            return ValueTask.CompletedTask;
        }
    }
}
