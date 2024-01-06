namespace LiveStreamingServerNet.Rtmp
{
    public static class RtmpConstants
    {
        public static readonly byte[] ServerVersion = [1, 0, 0, 0];

        public const uint DefaultChunkSize = 128;

        public const uint ProtocolControlMessageChunkStreamId = 2;
        public const uint ProtocolControlMessageStreamId = 0;

        public const uint UserControlMessageChunkStreamId = 2;
        public const uint UserControlMessageStreamId = 0;

        public const uint AudioMessageChunkStreamId = 4;
        public const uint VideoMessageChunkStreamId = 5;
        public const uint DataMessageChunkStreamId = 6;
    }

    public static class RtmpUserControlMessageTypes
    {
        public const ushort StreamBegin = 0;
        public const ushort StreamEof = 1;
        public const ushort StreamDry = 2;
        public const ushort SetBufferLength = 3;
        public const ushort StreamIsRecorded = 4;
        public const ushort PingRequest = 6;
        public const ushort PingResponse = 7;
    }

    public enum RtmpPeerBandwidthLimitType : byte
    {
        Hard = 0,
        Soft = 1,
        Dynamic = 2
    }

    public static class RtmpDataMessageConstants
    {
        public const string SetDataFrame = "@setDataFrame";
        public const string OnMetaData = "onMetaData";
    }

    public static class RtmpStatusCodes
    {
        public const string ConnectSuccess = "NetConnection.Connect.Success";
        public const string ConnectRejected = "NetConnection.Connect.Rejected";
        public const string StreamReset = "NetStream.Play.Reset";
        public const string StreamStart = "NetStream.Play.Start";
        public const string StreamPause = "NetStream.Pause.Notify";
        public const string StreamUnpause = "NetStream.Unpause.Notify";
        public const string PublishStart = "NetStream.Publish.Start";
        public const string DataStart = "NetStream.Data.Start";
        public const string UnpublishSuccess = "NetStream.Unpublish.Success";
        public const string PublishUnauthorized = "NetStream.publish.Unauthorized";
        public const string PublishBadName = "NetStream.Publish.BadName";
        public const string PublishBadConnection = "NetStream.Publish.BadConnection";
        public const string PlayStart = "NetStream.Play.Start";
        public const string PlayBadConnection = "NetStream.Play.BadConnection";
        public const string PlayUnpublishNotify = "NetStream.Play.UnpublishNotify";
    }

    public static class RtmpArgumentNames
    {
        public const string Level = "level";
        public const string Code = "code";
        public const string Description = "description";
        public const string Details = "details";
        public const string ClientId = "clientid";
        public const string ObjectEncoding = "objectEncoding";
    }

    public static class RtmpArgumentValues
    {
        public const string Error = "error";
        public const string Status = "status";
    }

    public enum VideoFrameType : byte
    {
        KeyFrame = 1,
        InterFrame = 2,
        DisposableInterFrame = 3,
        GeneratedKeyFrame = 4,
        VideoInfoOrCommandFrame = 5
    }

    public enum VideoCodecId : byte
    {
        SorensonH263 = 2,
        ScreenVideo = 3,
        On2VP6 = 4,
        On2VP6WithAlphaChannel = 5,
        ScreenVideoVersion2 = 6,
        AVC = 7
    }

    public enum AVCPacketType : byte
    {
        SequenceHeader = 0,
        NALU = 1,
        EndOfSequence = 2
    }

    public enum AudioSoundFormat
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
        MP38kHz = 14,
        DeviceSpecificSound = 15
    }

    public enum AACPacketType
    {
        SequenceHeader = 0,
        Raw = 1
    }
}
