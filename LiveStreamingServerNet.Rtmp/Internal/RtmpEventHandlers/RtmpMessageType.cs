namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers
{
    internal static class RtmpMessageType
    {
        /* protocol control messages */
        public const byte SetChunkSize = 1;
        public const byte AbortMessage = 2;
        public const byte Acknowledgement = 3;
        public const byte WindowAcknowledgementSize = 5;
        public const byte SetClientBandwidth = 6;

        /* user control messages */
        public const byte UserControlMessage = 4;

        /* command messages */
        public const byte CommandMessageAmf0 = 20;
        public const byte CommandMessageAmf3 = 17;

        /* data messages */
        public const byte DataMessageAmf0 = 18;
        public const byte DataMessageAmf3 = 15;

        /* shared object messages */
        public const byte SharedObjectMessageAmf0 = 19;
        public const byte SharedObjectMessageAmf3 = 16;

        /* audio message */
        public const byte AudioMessage = 8;

        /* video message */
        public const byte VideoMessage = 9;

        /* aggregate message */
        public const byte AggregateMessage = 22;
    }
}
