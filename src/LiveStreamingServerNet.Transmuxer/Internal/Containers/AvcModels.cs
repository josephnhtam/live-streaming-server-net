namespace LiveStreamingServerNet.Transmuxer.Internal.Containers
{
    internal record AvcSequenceHeader(
        byte ConfigVersion,
        byte AvcProfileIndication,
        byte ProfileCompatibility,
        byte AvcLevelIndication,
        byte NALULengthSizeMinusOne,
        byte[] SPS,
        byte[] PPS
     );
}
