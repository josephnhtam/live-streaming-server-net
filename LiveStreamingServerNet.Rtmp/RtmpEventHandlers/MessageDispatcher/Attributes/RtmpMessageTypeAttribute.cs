namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class RtmpMessageTypeAttribute : Attribute
    {
        public readonly byte MessageTypeId;

        public RtmpMessageTypeAttribute(byte messageTypeId)
        {
            MessageTypeId = messageTypeId;
        }
    }
}
