﻿using System.Buffers.Binary;
using System.Text;

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
        HEVC = 12,
        AV1 = 13
    }

    public enum VideoPacketType : byte
    {
        SequenceStart = 0,
        CodedFrames = 1,
        SequenceEnd = 2,
        CodedFramesX = 3,
        MetaData = 4,
        MPEG2TSSequenceStart = 5
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

    public static class VideoFourCC
    {
        public readonly static uint AV1 = BinaryPrimitives.ReadUInt32BigEndian(Encoding.ASCII.GetBytes("av01"));
        public readonly static uint HEVC = BinaryPrimitives.ReadUInt32BigEndian(Encoding.ASCII.GetBytes("hvc1"));
    }

    public static class RtmpArguments
    {
        public const string Level = "level";
        public const string Code = "code";
        public const string Description = "description";
        public const string Details = "details";
        public const string ClientId = "clientid";
        public const string ObjectEncoding = "objectEncoding";
    }

    public static class RtmpConnectionStatusCodes
    {
        public const string ConnectSuccess = "NetConnection.Connect.Success";
        public const string ConnectRejected = "NetConnection.Connect.Rejected";
    }

    public static class RtmpStreamStatusCodes
    {
        public const string PauseNotify = "NetStream.Pause.Notify";
        public const string UnpauseNotify = "NetStream.Unpause.Notify";
        public const string PublishStart = "NetStream.Publish.Start";
        public const string DataStart = "NetStream.Data.Start";
        public const string UnpublishSuccess = "NetStream.Unpublish.Success";
        public const string PublishUnauthorized = "NetStream.Publish.Unauthorized";
        public const string PublishBadName = "NetStream.Publish.BadName";
        public const string PublishBadConnection = "NetStream.Publish.BadConnection";
        public const string PlayReset = "NetStream.Play.Reset";
        public const string PlayStart = "NetStream.Play.Start";
        public const string PlayFailed = "NetStream.Play.Failed";
        public const string PlayBadConnection = "NetStream.Play.BadConnection";
        public const string PlayUnpublishNotify = "NetStream.Play.UnpublishNotify";
    }

    public static class RtmpStatusLevels
    {
        public const string Error = "error";
        public const string Status = "status";
    }

    public enum AmfEncodingType
    {
        Amf0,
        Amf3
    }
}
