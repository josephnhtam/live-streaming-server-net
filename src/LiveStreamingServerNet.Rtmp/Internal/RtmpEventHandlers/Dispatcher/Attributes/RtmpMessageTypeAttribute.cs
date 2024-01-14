namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes
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
