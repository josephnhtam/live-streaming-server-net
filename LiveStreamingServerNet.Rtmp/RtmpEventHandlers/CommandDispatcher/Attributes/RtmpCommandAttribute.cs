namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class RtmpCommandAttribute : Attribute
    {
        public string Name { get; }
        public RtmpCommandAttribute(string name)
        {
            Name = name;
        }
    }
}
