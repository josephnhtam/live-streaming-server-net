namespace LiveStreamingServerNet.Transmuxer.Internal.Containers
{
    internal record AACSequenceHeader(
        byte ObjectType,
        byte SampleRateIndex,
        byte ChannelConfig
    );
}
