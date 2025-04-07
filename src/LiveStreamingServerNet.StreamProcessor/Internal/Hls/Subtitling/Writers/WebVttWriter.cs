using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt.Builders;
using LiveStreamingServerNet.StreamProcessor.Internal.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Writers
{
    internal class WebVttWriter : IWebVttWriter
    {
        public Task WriteAsync(string webVttOutputPath, IReadOnlyList<SubtitleCue> cues, CancellationToken cancellationToken = default)
        {
            var webVttBuilder = new WebVttBuilder();

            foreach (var cue in cues)
            {
                webVttBuilder.AddCue(cue);
            }

            var webVtt = webVttBuilder.Build();

            return FileHelper.WriteToFileAsync(webVttOutputPath, webVtt, cancellationToken);
        }
    }
}
