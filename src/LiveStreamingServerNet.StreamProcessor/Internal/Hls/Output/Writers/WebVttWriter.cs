using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Utilities;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt.Builders;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers
{
    public class WebVttWriter : IWebVttWriter
    {
        public Task WriteAsync(string webVttOutputPath, IEnumerable<SubtitleCue> cues, CancellationToken cancellationToken = default)
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
