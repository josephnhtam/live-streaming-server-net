namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
{
    internal record struct SeqSegmentPartial(string FilePath, uint SequenceNumber);
    internal record struct SeqSegment(string FilePath, uint SequenceNumber, uint Timestamp, uint Duration);
}
