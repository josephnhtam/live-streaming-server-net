namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler.PayloadHandling
{
    public static class RtmpMessages
    {
        public const byte SetChunkSize = 0x01;
        public const byte AbortMessage = 0x02;
        public const byte Acknowledgement = 0x03;
        public const byte UserControlMessage = 0x04;
        public const byte WindowAcknowledgementSize = 0x05;
        public const byte SetPeerBandwidth = 0x06;
        public const byte AudioMessage = 0x08;
        public const byte VideoMessage = 0x09;
        public const byte DataMessage3 = 0x0F;
        public const byte SharedObjectMessage3 = 0x10;
        public const byte CommandMessage3 = 0x11;
        public const byte AggregateMessage = 0x16;
    }

    public enum RtmpMessageTypes : byte
    {
        /* protocol control messages */
        SetChunkSize = 1,
        AbortMessage = 2,
        Acknowledgement = 3,
        WindowAcknowledgementSize = 5,
        SetPeerBandwidth = 6,

        /* user control messages */
        AudioMessage = 0x08,
        VideoMessage = 0x09,
        DataMessage3 = 0x0F,
        SharedObjectMessage3 = 0x10,
        CommandMessage3 = 0x11,
        AggregateMessage = 0x16
    }
}
