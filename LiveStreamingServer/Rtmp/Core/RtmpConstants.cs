namespace LiveStreamingServer.Rtmp.Core
{
    public static class RtmpConstants
    {
        public const uint DefaultChunkSize = 128;
        public const uint DefaultPeerBandwidth = 500_000;
        public const uint DefaultInAcknowledgementWindowSize = 250_000;
        public const uint DefaultOutAcknowledgementWindowSize = 250_000;

        public const uint ProtocolControlMessageChunkStreamId = 2;
        public const uint ProtocolControlMessageStreamId = 0;
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
}
