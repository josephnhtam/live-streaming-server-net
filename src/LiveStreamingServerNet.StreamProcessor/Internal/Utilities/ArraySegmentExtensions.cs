namespace LiveStreamingServerNet.StreamProcessor.Internal.Utilities
{
    internal static class ArraySegmentExtensions
    {
        public static int ReadInt16BigEndian(this ArraySegment<byte> buffer)
        {
            return (buffer[0] << 8) | buffer[1];
        }

        public static int ReadInt24BigEndian(this ArraySegment<byte> buffer)
        {
            return (buffer[0] << 16) | (buffer[1] << 8) | buffer[2];
        }

        public static int ReadInt32BigEndian(this ArraySegment<byte> buffer)
        {
            return (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
        }

        public static int ReadInt48BigEndian(this ArraySegment<byte> buffer)
        {
            return (buffer[0] << 40) | (buffer[1] << 32) | (buffer[2] << 24) | (buffer[3] << 16) | (buffer[4] << 8) | buffer[5];
        }
    }
}
