﻿namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RtmpCommandAttribute : Attribute
    {
        public string Name { get; }
        public RtmpCommandAttribute(string name)
        {
            Name = name;
        }
    }
}