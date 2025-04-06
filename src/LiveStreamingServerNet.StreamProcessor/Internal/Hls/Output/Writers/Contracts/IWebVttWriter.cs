using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts
{
    internal interface IWebVttWriter
    {
        Task WriteAsync(string webVttOutputPath, IEnumerable<SubtitleCue> cues, CancellationToken cancellationToken = default);
    }
}
