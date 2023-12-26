namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler.PayloadHandling
{
    public enum RtmpMessageTypes : byte
    {
        /* protocol control messages */
        SetChunkSize = 1,
        AbortMessage = 2,
        Acknowledgement = 3,
        WindowAcknowledgementSize = 5,
        SetPeerBandwidth = 6,

        /* user control messages */
        UserControlMessage = 4,

        /* command messages */
        CommandMessageAmf0 = 20,
        CommandMessageAmf3 = 17,

        /* data messages */
        DataMessageAmf0 = 18,
        DataMessageAmf3 = 15,

        /* shared object messages */
        SharedObjectMessageAmf0 = 19,
        SharedObjectMessageAmf3 = 16,

        /* audio message */
        AudioMessage = 8,

        /* video message */
        VideoMessage = 9,

        /* aggregate message */
        AggregateMessage = 22,
    }
}
