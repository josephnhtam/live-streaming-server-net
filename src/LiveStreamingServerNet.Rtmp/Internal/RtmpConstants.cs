namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal static class RtmpConstants
    {
        public static readonly byte[] ServerVersion = { 1, 0, 0, 0 };

        public const uint DefaultChunkSize = 128;

        public const uint ProtocolControlMessageChunkStreamId = 2;
        public const uint ProtocolControlMessageStreamId = 0;

        public const uint UserControlMessageChunkStreamId = 2;
        public const uint UserControlMessageStreamId = 0;

        public const uint AudioMessageChunkStreamId = 4;
        public const uint VideoMessageChunkStreamId = 5;
        public const uint DataMessageChunkStreamId = 6;
    }

    internal static class RtmpUserControlMessageTypes
    {
        public const ushort StreamBegin = 0;
        public const ushort StreamEof = 1;
        public const ushort StreamDry = 2;
        public const ushort SetBufferLength = 3;
        public const ushort StreamIsRecorded = 4;
        public const ushort PingRequest = 6;
        public const ushort PingResponse = 7;
    }

    internal enum RtmpClientBandwidthLimitType : byte
    {
        Hard = 0,
        Soft = 1,
        Dynamic = 2
    }

    internal static class RtmpDataMessageConstants
    {
        public const string SetDataFrame = "@setDataFrame";
        public const string OnMetaData = "onMetaData";
    }

    internal static class RtmpStatusCodes
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

    internal static class RtmpArgumentNames
    {
        public const string Level = "level";
        public const string Code = "code";
        public const string Description = "description";
        public const string Details = "details";
        public const string ClientId = "clientid";
        public const string ObjectEncoding = "objectEncoding";
    }

    internal static class RtmpArgumentValues
    {
        public const string Error = "error";
        public const string Status = "status";
    }
}
