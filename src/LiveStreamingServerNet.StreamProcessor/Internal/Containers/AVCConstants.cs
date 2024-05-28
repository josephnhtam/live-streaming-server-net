namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
{
    internal static class AVCConstants
    {
        public readonly static ArraySegment<byte> NALU_AUD = new ArraySegment<byte>(new byte[] { 0, 0, 0, 1, 0x09, 0xf0 });
        public readonly static ArraySegment<byte> NALU_StartCode = new ArraySegment<byte>(new byte[] { 0, 0, 0, 1 });

        public const int H264Frequency = 90;
    }
}
