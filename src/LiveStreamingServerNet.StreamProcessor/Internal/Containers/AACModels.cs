namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
{
    internal record AACSequenceHeader(
        byte ObjectType,
        byte SampleRateIndex,
        byte ChannelConfig
    );
}
