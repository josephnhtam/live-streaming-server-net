namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
{
    internal record struct SegmentPartial(string FilePath, uint SequenceNumber);
    internal record struct Segment(string FilePath, uint SequenceNumber, uint Timestamp, uint Duration);
}
