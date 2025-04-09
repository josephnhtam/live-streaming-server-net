using LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt.Builders.Contracts;
using System.Text;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt.Builders
{
    internal class WebVttBuilder : IWebVttBuilder
    {
        private readonly List<SubtitleCue> _cues;

        public WebVttBuilder()
        {
            _cues = new List<SubtitleCue>();
        }

        public IWebVttBuilder AddCue(SubtitleCue cue)
        {
            _cues.Add(cue);
            return this;
        }

        public string Build()
        {
            var sb = new StringBuilder();

            sb.AppendLine("WEBVTT");
            sb.AppendLine();

            foreach (var cue in _cues)
            {
                string startTime = FormatTime(cue.Timestamp);
                string endTime = FormatTime(cue.Timestamp + cue.Duration);

                sb.AppendLine($"{startTime} --> {endTime}");
                sb.AppendLine(cue.Text);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string FormatTime(TimeSpan time)
        {
            return time.ToString(@"hh\:mm\:ss\.fff");
        }
    }
}
