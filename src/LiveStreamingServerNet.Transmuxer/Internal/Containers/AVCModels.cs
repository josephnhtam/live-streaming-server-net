namespace LiveStreamingServerNet.Transmuxer.Internal.Containers
{
    internal record AVCSequenceHeader(
        byte ConfigVersion,
        byte AVCProfileIndication,
        byte ProfileCompatibility,
        byte AVCLevelIndication,
        byte NALULengthSizeMinusOne,
        byte[] SPS,
        byte[] PPS
     );
}
