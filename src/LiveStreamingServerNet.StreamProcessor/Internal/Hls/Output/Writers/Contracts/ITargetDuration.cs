using LiveStreamingServerNet.StreamProcessor.Internal.Containers;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts
{
    internal interface ITargetDuration
    {
        TimeSpan Calculate(IEnumerable<SeqSegment> segments);
    }
}
