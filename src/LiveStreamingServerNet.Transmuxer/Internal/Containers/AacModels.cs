namespace LiveStreamingServerNet.Transmuxer.Internal.Containers
{
    internal record AacSequenceHeader(
        byte ObjectType,
        byte SampleRateIndex,
        byte ChannelConfig
    );
}
