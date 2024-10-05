namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
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

    internal record HEVCSequenceHeader(
       byte ConfigVersion,
       byte GeneralProfileSpace,
       byte GeneralTierFlag,
       byte GeneralProfileIdc,
       uint GeneralProfileCompatibilityFlags,
       ulong GeneralConstraintIndicatorFlags,
       byte GeneralLevelIdc,
       uint MinSpatialSegmentationIdc,
       byte ParallelismType,
       byte ChromaFormat,
       byte BitDepthLumaMinus8,
       byte BitDepthChromaMinus8,
       ushort AvgFrameRate,
       byte ConstantFrameRate,
       byte NumTemporalLayers,
       byte TemporalIdNested,
       byte NALULengthSizeMinusOne,
       byte[] VPS,
       byte[] SPS,
       byte[] PPS
   );
}
