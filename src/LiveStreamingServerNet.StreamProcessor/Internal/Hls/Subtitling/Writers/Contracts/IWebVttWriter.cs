using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Writers.Contracts
{
    internal interface IWebVttWriter
    {
        Task WriteAsync(string webVttOutputPath, IReadOnlyList<SubtitleCue> cues, CancellationToken cancellationToken = default);
    }
}
