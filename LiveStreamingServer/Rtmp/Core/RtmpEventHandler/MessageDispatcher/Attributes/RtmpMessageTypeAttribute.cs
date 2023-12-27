namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RtmpMessageTypeAttribute : Attribute
    {
        public readonly byte MessageTypeId;

        public RtmpMessageTypeAttribute(byte messageTypeId)
        {
            MessageTypeId = messageTypeId;
        }
    }
}
