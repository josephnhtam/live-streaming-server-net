using LiveStreamingServer.Newtorking.Contracts;

namespace LiveStreamingServer.Rtmp.Core.Extensions
{
    public static class NetBufferExtensions
    {
        public static uint ReadUInt24BigEndian(this INetBuffer buffer)
        {
            return (uint)((buffer.ReadByte() << 16) | (buffer.ReadByte() << 8) | buffer.ReadByte());
        }

        public static uint ReadUInt32BigEndian(this INetBuffer buffer)
        {
            return (uint)((buffer.ReadByte() << 24) | (buffer.ReadByte() << 16) | (buffer.ReadByte() << 8) | buffer.ReadByte());
        }

        public static void WriteUInt24BigEndian(this INetBuffer buffer, uint value)
        {
            buffer.Write((byte)(value >> 16));
            buffer.Write((byte)(value >> 8));
            buffer.Write((byte)value);
        }

        public static void WriteUInt32BigEndian(this INetBuffer buffer, uint value)
        {
            buffer.Write((byte)(value >> 24));
            buffer.Write((byte)(value >> 16));
            buffer.Write((byte)(value >> 8));
            buffer.Write((byte)value);
        }
    }
}
