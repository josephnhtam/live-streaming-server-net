namespace LiveStreamingServerNet.Utilities.Buffers.Contracts
{
    public interface IDataBufferReader : IDisposable
    {
        int Position { get; set; }
        int Size { get; }

        bool ReadBoolean();
        byte ReadByte();
        void ReadBytes(byte[] buffer, int index, int count);
        void ReadBytes(Span<byte> buffer);
        byte[] ReadBytes(int count);
        char ReadChar();
        double ReadDouble();
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();
        float ReadSingle();
        ushort ReadUInt16();
        uint ReadUInt32();
        ulong ReadUInt64();
        ushort ReadUInt16BigEndian();
        uint ReadUInt24BigEndian();
        uint ReadUInt32BigEndian();
        ulong ReadUInt64BigEndian();
        short ReadInt16BigEndian();
        int ReadInt24BigEndian();
        int ReadInt32BigEndian();
        long ReadInt64BigEndian();
        string ReadUtf8String(int length);

        ReadOnlySpan<byte> AsReadOnlySpan();
        ReadOnlySpan<byte> AsReadOnlySpan(int offset);
        ReadOnlySpan<byte> AsReadOnlySpan(int offset, int length);
        ReadOnlyMemory<byte> AsReadOnlyMemory();
        ReadOnlyMemory<byte> AsReadOnlyMemory(int offset);
        ReadOnlyMemory<byte> AsReadOnlyMemory(int offset, int length);
    }
}
