namespace LiveStreamingServerNet.Rtmp
{
    public enum MediaType
    {
        Video,
        Audio
    }

    public enum VideoFrameType : byte
    {
        KeyFrame = 1,
        InterFrame = 2,
        DisposableInterFrame = 3,
        GeneratedKeyFrame = 4,
        VideoInfoOrCommandFrame = 5
    }

    public enum VideoCodec : byte
    {
        SorensonH263 = 2,
        ScreenVideo = 3,
        On2VP6 = 4,
        On2VP6WithAlphaChannel = 5,
        ScreenVideoVersion2 = 6,
        AVC = 7,
        HVC = 12,
        Opus = 13
    }

    public enum AVCPacketType : byte
    {
        SequenceHeader = 0,
        NALU = 1,
        EndOfSequence = 2
    }

    public enum AudioCodec
    {
        LinearPCMPlatformEndian = 0,
        ADPCM = 1,
        MP3 = 2,
        LinearPCMLittleEndian = 3,
        Nellymoser16kHzMono = 4,
        Nellymoser8kHzMono = 5,
        Nellymoser = 6,
        G711ALawLogarithmicPCM = 7,
        G711MuLawLogarithmicPCM = 8,
        Reserved = 9,
        AAC = 10,
        Speex = 11,
        Opus = 13,
        MP38kHz = 14,
        DeviceSpecificSound = 15
    }

    public enum AACPacketType
    {
        SequenceHeader = 0,
        Raw = 1
    }
}
