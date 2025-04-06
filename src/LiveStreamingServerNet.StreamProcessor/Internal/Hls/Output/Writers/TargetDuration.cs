using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers
{
    internal class MaximumTargetDuration : ITargetDuration
    {
        public TimeSpan Calculate(IEnumerable<SeqSegment> segments)
        {
            if (!segments.Any())
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(segments.Max(s => s.Duration));
        }
    }

    internal class MinimumTargetDuration : ITargetDuration
    {
        public TimeSpan Calculate(IEnumerable<SeqSegment> segments)
        {
            if (!segments.Any())
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(segments.Min(s => s.Duration));
        }
    }

    internal class AverageTargetDuration : ITargetDuration
    {
        public TimeSpan Calculate(IEnumerable<SeqSegment> segments)
        {
            if (!segments.Any())
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(segments.Average(s => s.Duration));
        }
    }

    internal class FixedTargetDuration : ITargetDuration
    {
        private readonly TimeSpan _fixedValue;

        public FixedTargetDuration(TimeSpan fixedValue)
        {
            _fixedValue = fixedValue;
        }

        public TimeSpan Calculate(IEnumerable<SeqSegment> segments)
        {
            return _fixedValue;
        }
    }
}
